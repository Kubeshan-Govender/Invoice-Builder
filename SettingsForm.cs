using InvoiceBuilder.Models;
using InvoiceBuilder.Services;
using InvoiceBuilder.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace InvoiceBuilder
{
    public class SettingsForm : Form
    {
        private readonly AppSettingsService _settingsService = new();
        private readonly Dictionary<string, TextBox> _fields = new();
        private AppSettings _settings = new();

        public SettingsForm()
        {
            Text = "Admin Settings";
            Width = 620;
            Height = 640;
            MinimumSize = new Size(560, 560);
            StartPosition = FormStartPosition.CenterParent;

            BuildLayout();
            AppStyles.ApplyForm(this);
            LoadSettings();
        }

        private void BuildLayout()
        {
            var main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(12)
            };

            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            main.RowStyles.Add(new RowStyle());

            var fields = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                Padding = new Padding(0, 0, 0, 8)
            };

            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            AddField(fields, nameof(AppSettings.DefaultVehicleRegistration), "Default Vehicle Reg");
            AddField(fields, nameof(AppSettings.StatementBillToName), "Statement Bill To");
            AddField(fields, nameof(AppSettings.StatementEmail), "Statement Email");
            AddField(fields, nameof(AppSettings.StatementPhone), "Statement Phone");
            AddField(fields, nameof(AppSettings.BankName), "Bank Name");
            AddField(fields, nameof(AppSettings.AccountHolder), "Account Holder");
            AddField(fields, nameof(AppSettings.AccountNumber), "Account Number");
            AddField(fields, nameof(AppSettings.AccountType), "Account Type");
            AddField(fields, nameof(AppSettings.BranchCode), "Branch Code");
            AddField(fields, nameof(AppSettings.EmailDefaultRecipient), "Default Email Recipient");
            AddField(fields, nameof(AppSettings.EmailSenderName), "Email Sender Name");
            AddField(fields, nameof(AppSettings.EmailSenderCompanyName), "Email Sender Company");
            AddField(fields, nameof(AppSettings.AdminPassword), "Admin Password", usePassword: true);

            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true
            };

            var save = new Button { Text = "Save Settings", Width = 140, Height = 34 };
            var cancel = new Button { Text = "Cancel", Width = 100, Height = 34 };
            save.Click += (_, _) => SaveSettings();
            cancel.Click += (_, _) => Close();

            footer.Controls.Add(save);
            footer.Controls.Add(cancel);

            main.Controls.Add(fields, 0, 0);
            main.Controls.Add(footer, 0, 1);
            Controls.Add(main);

            AppStyles.ApplyButton(save, ButtonRole.Primary);
            AppStyles.ApplyButton(cancel, ButtonRole.Secondary);
        }

        private void AddField(TableLayoutPanel panel, string key, string label, bool usePassword = false)
        {
            var row = panel.RowCount++;
            panel.RowStyles.Add(new RowStyle());

            var field = new TextBox
            {
                Dock = DockStyle.Fill,
                UseSystemPasswordChar = usePassword
            };

            panel.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
            panel.Controls.Add(field, 1, row);
            _fields[key] = field;
        }

        private void LoadSettings()
        {
            _settings = _settingsService.Load();

            _fields[nameof(AppSettings.DefaultVehicleRegistration)].Text = _settings.DefaultVehicleRegistration;
            _fields[nameof(AppSettings.StatementBillToName)].Text = _settings.StatementBillToName;
            _fields[nameof(AppSettings.StatementEmail)].Text = _settings.StatementEmail;
            _fields[nameof(AppSettings.StatementPhone)].Text = _settings.StatementPhone;
            _fields[nameof(AppSettings.BankName)].Text = _settings.BankName;
            _fields[nameof(AppSettings.AccountHolder)].Text = _settings.AccountHolder;
            _fields[nameof(AppSettings.AccountNumber)].Text = _settings.AccountNumber;
            _fields[nameof(AppSettings.AccountType)].Text = _settings.AccountType;
            _fields[nameof(AppSettings.BranchCode)].Text = _settings.BranchCode;
            _fields[nameof(AppSettings.EmailDefaultRecipient)].Text = _settings.EmailDefaultRecipient;
            _fields[nameof(AppSettings.EmailSenderName)].Text = _settings.EmailSenderName;
            _fields[nameof(AppSettings.EmailSenderCompanyName)].Text = _settings.EmailSenderCompanyName;
            _fields[nameof(AppSettings.AdminPassword)].Text = _settings.AdminPassword;
        }

        private void SaveSettings()
        {
            _settings.DefaultVehicleRegistration = ReadField(nameof(AppSettings.DefaultVehicleRegistration));
            _settings.StatementBillToName = ReadField(nameof(AppSettings.StatementBillToName));
            _settings.StatementEmail = ReadField(nameof(AppSettings.StatementEmail));
            _settings.StatementPhone = ReadField(nameof(AppSettings.StatementPhone));
            _settings.BankName = ReadField(nameof(AppSettings.BankName));
            _settings.AccountHolder = ReadField(nameof(AppSettings.AccountHolder));
            _settings.AccountNumber = ReadField(nameof(AppSettings.AccountNumber));
            _settings.AccountType = ReadField(nameof(AppSettings.AccountType));
            _settings.BranchCode = ReadField(nameof(AppSettings.BranchCode));
            _settings.EmailDefaultRecipient = ReadField(nameof(AppSettings.EmailDefaultRecipient));
            _settings.EmailSenderName = ReadField(nameof(AppSettings.EmailSenderName));
            _settings.EmailSenderCompanyName = ReadField(nameof(AppSettings.EmailSenderCompanyName));
            _settings.AdminPassword = ReadField(nameof(AppSettings.AdminPassword));

            if (string.IsNullOrWhiteSpace(_settings.AdminPassword))
            {
                MessageBox.Show("Admin password cannot be blank.", "Admin Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _settingsService.Save(_settings);
            DialogResult = DialogResult.OK;
            Close();
        }

        private string ReadField(string key)
        {
            return _fields.TryGetValue(key, out var field) ? field.Text.Trim() : string.Empty;
        }

        public static bool PromptForAdmin(IWin32Window owner)
        {
            var settings = new AppSettingsService().Load();
            using var dialog = new Form
            {
                Text = "Admin Unlock",
                Width = 360,
                Height = 155,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent
            };

            var input = new TextBox
            {
                Dock = DockStyle.Fill,
                UseSystemPasswordChar = true
            };

            var ok = new Button { Text = "Unlock", DialogResult = DialogResult.OK, Width = 90 };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 90 };
            var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
            footer.Controls.Add(ok);
            footer.Controls.Add(cancel);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(12) };
            layout.RowStyles.Add(new RowStyle());
            layout.RowStyles.Add(new RowStyle());
            layout.RowStyles.Add(new RowStyle());
            layout.Controls.Add(new Label { Text = "Enter admin password", AutoSize = true }, 0, 0);
            layout.Controls.Add(input, 0, 1);
            layout.Controls.Add(footer, 0, 2);
            dialog.Controls.Add(layout);
            dialog.AcceptButton = ok;
            dialog.CancelButton = cancel;

            AppStyles.ApplyForm(dialog);
            AppStyles.ApplyButton(ok, ButtonRole.Primary);
            AppStyles.ApplyButton(cancel, ButtonRole.Secondary);

            return dialog.ShowDialog(owner) == DialogResult.OK
                && string.Equals(input.Text, settings.AdminPassword, StringComparison.Ordinal);
        }
    }
}
