using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using InvoiceBuilder.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InvoiceBuilder.Services
{
    public class EmailDraftService
    {
        private const string ApplicationName = "Invoice Builder";
        private const string CredentialsFileName = "credentials.json";
        private readonly InvoiceLayoutProfile _layout;

        public EmailDraftService()
            : this(InvoiceLayoutProfiles.Active)
        {
        }

        internal EmailDraftService(InvoiceLayoutProfile layout)
        {
            _layout = layout;
        }

        public async Task<string> CreateStatementEmailDraftAsync(
            Statement statement,
            string statementPath,
            IEnumerable<string> invoicePaths,
            CancellationToken cancellationToken = default)
        {
            var recipient = _layout.EmailDefaultRecipient?.Trim();
            if (string.IsNullOrWhiteSpace(recipient))
            {
                throw new InvalidOperationException("Set a Default Email Recipient in Admin Settings before creating a Gmail draft.");
            }

            var attachmentPaths = invoicePaths
                .Append(statementPath)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Where(File.Exists)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!attachmentPaths.Contains(statementPath, StringComparer.OrdinalIgnoreCase))
            {
                throw new FileNotFoundException("The generated statement PDF could not be found.", statementPath);
            }

            var credentialsPath = Path.Combine(AppContext.BaseDirectory, CredentialsFileName);
            if (!File.Exists(credentialsPath))
            {
                throw new FileNotFoundException(
                    $"Gmail sign-in is not configured. Download a Desktop app OAuth client JSON from Google Cloud and save it as '{CredentialsFileName}' beside Invoice Builder.",
                    credentialsPath);
            }

            UserCredential credential;
            await using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var secrets = GoogleClientSecrets.FromStream(stream).Secrets;
                var tokenPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "InvoiceBuilder",
                    "GoogleOAuth");

                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secrets,
                    new[] { GmailService.Scope.GmailCompose },
                    "user",
                    cancellationToken,
                    new FileDataStore(tokenPath, true));
            }

            var gmail = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            var subject = $"Invoices and Statement for vsl {statement.Vessel}";
            var rawMessage = BuildMimeMessage(recipient, subject, BuildBody(), attachmentPaths);
            var draft = await gmail.Users.Drafts.Create(new Draft
            {
                Message = new Message { Raw = Base64UrlEncode(rawMessage) }
            }, "me").ExecuteAsync(cancellationToken);

            try
            {
                Process.Start(new ProcessStartInfo("https://mail.google.com/mail/u/0/#drafts")
                {
                    UseShellExecute = true
                });
            }
            catch
            {
                // The draft is already saved; it remains available from Gmail's Drafts folder.
            }

            return $"Gmail draft created with {attachmentPaths.Count} attachment(s). Gmail Drafts was opened so you can review and send it. Draft ID: {draft.Id}";
        }

        private string BuildBody()
        {
            return string.Join(Environment.NewLine,
                "Good Day",
                string.Empty,
                "Please see attached invoice PDFs, cargo spreadsheets, and statement as per above.",
                string.Empty,
                "kind regards",
                _layout.EmailSenderName,
                _layout.EmailSenderCompanyName);
        }

        private static byte[] BuildMimeMessage(
            string recipient,
            string subject,
            string body,
            IReadOnlyList<string> attachmentPaths)
        {
            var boundary = $"----=_InvoiceBuilder_{Guid.NewGuid():N}";
            var message = new StringBuilder();
            message.AppendLine($"To: {SanitizeHeader(recipient)}");
            message.AppendLine($"Subject: {EncodeHeader(subject)}");
            message.AppendLine("MIME-Version: 1.0");
            message.AppendLine($"Content-Type: multipart/mixed; boundary=\"{boundary}\"");
            message.AppendLine();
            message.AppendLine($"--{boundary}");
            message.AppendLine("Content-Type: text/plain; charset=utf-8");
            message.AppendLine("Content-Transfer-Encoding: base64");
            message.AppendLine();
            AppendBase64Lines(message, Encoding.UTF8.GetBytes(body));

            foreach (var path in attachmentPaths)
            {
                var fileName = SanitizeFileName(Path.GetFileName(path));
                message.AppendLine($"--{boundary}");
                message.AppendLine($"Content-Type: {GetContentType(path)}; name=\"{fileName}\"");
                message.AppendLine("Content-Transfer-Encoding: base64");
                message.AppendLine($"Content-Disposition: attachment; filename=\"{fileName}\"");
                message.AppendLine();
                AppendBase64Lines(message, File.ReadAllBytes(path));
            }

            message.AppendLine($"--{boundary}--");
            return Encoding.UTF8.GetBytes(message.ToString());
        }

        private static void AppendBase64Lines(StringBuilder builder, byte[] bytes)
        {
            var base64 = Convert.ToBase64String(bytes);
            for (var offset = 0; offset < base64.Length; offset += 76)
            {
                builder.AppendLine(base64.Substring(offset, Math.Min(76, base64.Length - offset)));
            }
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        private static string EncodeHeader(string value)
        {
            var safeValue = SanitizeHeader(value);
            return safeValue.All(character => character <= 127)
                ? safeValue
                : $"=?utf-8?B?{Convert.ToBase64String(Encoding.UTF8.GetBytes(safeValue))}?=";
        }

        private static string SanitizeHeader(string value)
        {
            return value.Replace("\r", " ").Replace("\n", " ").Trim();
        }

        private static string SanitizeFileName(string value)
        {
            return value.Replace("\\", "_").Replace("/", "_").Replace("\"", "'")
                .Replace("\r", "_").Replace("\n", "_");
        }

        private static string GetContentType(string path)
        {
            return Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }
    }
}
