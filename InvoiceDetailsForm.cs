using System;
using System.Drawing;
using System.Windows.Forms;

namespace InvoiceBuilder
{
    internal sealed class InvoiceDetailsForm : Form
    {
        private readonly DateTimePicker _datePicker = new();
        private readonly TextBox _vesselTextBox = new();

        public DateTime InvoiceDate { get; private set; }
        public string VesselName { get; private set; } = string.Empty;

        public InvoiceDetailsForm(DateTime initialDate, string initialVessel)
        {
            Text = "Invoice Details";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(440, 226);
            Font = new Font("Segoe UI", 10F);

            var heading = new Label
            {
                Text = "Complete the required invoice details",
                Font = new Font("Segoe UI Semibold", 14F),
                AutoSize = true,
                Location = new Point(24, 20)
            };
            var help = new Label
            {
                Text = "These details will appear on the invoice and determine its save location.",
                ForeColor = Color.DimGray,
                AutoSize = true,
                Location = new Point(25, 52)
            };
            var dateLabel = new Label
            {
                Text = "Invoice date *",
                AutoSize = true,
                Location = new Point(26, 91)
            };
            _datePicker.Location = new Point(148, 86);
            _datePicker.Width = 265;
            _datePicker.Format = DateTimePickerFormat.Long;
            _datePicker.ShowCheckBox = true;
            _datePicker.Checked = false;
            _datePicker.Value = initialDate == default ? DateTime.Today : initialDate;

            var vesselLabel = new Label
            {
                Text = "Vessel name *",
                AutoSize = true,
                Location = new Point(26, 130)
            };
            _vesselTextBox.Location = new Point(148, 125);
            _vesselTextBox.Size = new Size(265, 27);
            _vesselTextBox.Text = initialVessel?.Trim() ?? string.Empty;

            var cancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Size = new Size(90, 36),
                Location = new Point(167, 174)
            };
            var generate = new Button
            {
                Text = "Generate Invoice",
                Size = new Size(146, 36),
                Location = new Point(267, 174),
                BackColor = Color.FromArgb(24, 85, 116),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            generate.FlatAppearance.BorderSize = 0;
            generate.Click += Generate_Click;

            Controls.AddRange(new Control[] { heading, help, dateLabel, _datePicker, vesselLabel, _vesselTextBox, cancel, generate });
            AcceptButton = generate;
            CancelButton = cancel;
        }

        private void Generate_Click(object sender, EventArgs e)
        {
            VesselName = _vesselTextBox.Text.Trim();
            if (!_datePicker.Checked)
            {
                MessageBox.Show("Select the invoice date to continue.", "Required details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _datePicker.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(VesselName))
            {
                MessageBox.Show("Enter the vessel name to continue.", "Required details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _vesselTextBox.Focus();
                return;
            }

            InvoiceDate = _datePicker.Value.Date;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
