using InvoiceBuilder.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InvoiceBuilder.Services
{
    public class EmailDraftService
    {
        private readonly InvoiceLayoutProfile _layout;

        public EmailDraftService()
            : this(InvoiceLayoutProfiles.Active)
        {
        }

        internal EmailDraftService(InvoiceLayoutProfile layout)
        {
            _layout = layout;
        }

        public string CreateStatementEmailDraft(Statement statement, string statementPath, IEnumerable<string> invoicePaths)
        {
            var attachmentPaths = invoicePaths
                .Append(statementPath)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Where(File.Exists)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var subject = $"Invoices and Statement for vsl {statement.Vessel}";
            var body = BuildBody();

            try
            {
                CreateOutlookDraft(subject, body, attachmentPaths);
                return "Email draft opened in Outlook.";
            }
            catch
            {
                var emlPath = CreateEmlDraft(subject, body, attachmentPaths, statementPath);
                Process.Start(new ProcessStartInfo(emlPath) { UseShellExecute = true });
                return $"Outlook was not available, so an email draft file was created:\n{emlPath}";
            }
        }

        private string BuildBody()
        {
            return string.Join(Environment.NewLine,
                "Good Day",
                string.Empty,
                "Please see attached copies as per above",
                string.Empty,
                "kind regards",
                _layout.EmailSenderName,
                _layout.EmailSenderCompanyName);
        }

        private void CreateOutlookDraft(string subject, string body, IReadOnlyList<string> attachmentPaths)
        {
            var outlookType = Type.GetTypeFromProgID("Outlook.Application")
                ?? throw new InvalidOperationException("Outlook is not installed.");

            var outlook = Activator.CreateInstance(outlookType)
                ?? throw new InvalidOperationException("Could not start Outlook.");

            var mail = outlookType.InvokeMember(
                "CreateItem",
                BindingFlags.InvokeMethod,
                null,
                outlook,
                new object[] { 0 }) ?? throw new InvalidOperationException("Could not create Outlook mail item.");

            var mailType = mail.GetType();
            mailType.InvokeMember("To", BindingFlags.SetProperty, null, mail, new object[] { _layout.EmailDefaultRecipient });
            mailType.InvokeMember("Subject", BindingFlags.SetProperty, null, mail, new object[] { subject });
            mailType.InvokeMember("Body", BindingFlags.SetProperty, null, mail, new object[] { body });

            var attachments = mailType.InvokeMember("Attachments", BindingFlags.GetProperty, null, mail, null)
                ?? throw new InvalidOperationException("Could not access Outlook attachments.");
            var attachmentsType = attachments.GetType();

            foreach (var path in attachmentPaths)
            {
                attachmentsType.InvokeMember("Add", BindingFlags.InvokeMethod, null, attachments, new object[] { path });
            }

            mailType.InvokeMember("Display", BindingFlags.InvokeMethod, null, mail, new object[] { false });
        }

        private string CreateEmlDraft(string subject, string body, IReadOnlyList<string> attachmentPaths, string statementPath)
        {
            var draftFolder = Path.GetDirectoryName(statementPath)
                ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Directory.CreateDirectory(draftFolder);

            var emlPath = Path.Combine(draftFolder, $"{SanitizeFileName(subject)}.eml");
            var boundary = $"----=_InvoiceBuilder_{Guid.NewGuid():N}";
            var message = new StringBuilder();

            message.AppendLine($"To: {_layout.EmailDefaultRecipient}");
            message.AppendLine($"Subject: {subject}");
            message.AppendLine("MIME-Version: 1.0");
            message.AppendLine($"Content-Type: multipart/mixed; boundary=\"{boundary}\"");
            message.AppendLine();
            message.AppendLine($"--{boundary}");
            message.AppendLine("Content-Type: text/plain; charset=utf-8");
            message.AppendLine("Content-Transfer-Encoding: 8bit");
            message.AppendLine();
            message.AppendLine(body);

            foreach (var path in attachmentPaths)
            {
                message.AppendLine($"--{boundary}");
                message.AppendLine($"Content-Type: application/pdf; name=\"{Path.GetFileName(path)}\"");
                message.AppendLine("Content-Transfer-Encoding: base64");
                message.AppendLine($"Content-Disposition: attachment; filename=\"{Path.GetFileName(path)}\"");
                message.AppendLine();
                message.AppendLine(Convert.ToBase64String(File.ReadAllBytes(path), Base64FormattingOptions.InsertLineBreaks));
            }

            message.AppendLine($"--{boundary}--");
            File.WriteAllText(emlPath, message.ToString(), Encoding.UTF8);
            return emlPath;
        }

        private static string SanitizeFileName(string input)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c.ToString(), string.Empty);
            }

            return input.Trim();
        }
    }
}
