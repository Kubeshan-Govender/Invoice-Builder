using InvoiceBuilder.Models;
using InvoiceBuilder.Repositories;
using InvoiceBuilder.Services;
using InvoiceBuilder.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace InvoiceBuilder
{
    public partial class InvoiceBuilder : Form
    {
        private readonly InvoiceNumberService _invoiceNumberService = new();
        private readonly InvoiceService _invoiceService = new();
        private readonly PdfInvoiceService _pdfInvoiceService = new();
        private readonly GeneratedInvoiceRepository _generatedInvoiceRepository = new();
        private readonly AppSettingsService _settingsService = new();
        private readonly Button _btnGenerateStatement = new();
        private readonly Button _btnSettings = new();

        public InvoiceBuilder()
        {
            InitializeComponent();
            ConfigureGrid();
            ApplyVisualStyle();
            WireEvents();
            StartNewInvoice();
        }

        private Invoice BuildInvoiceFromUI(bool validateSourceInvoiceNumbers = false)
        {
            var invoice = new Invoice
            {
                InvoiceNumber = int.Parse(lblInvoiceNum.Text),
                Date = dtpDate.Value,
                Vessel = txtVessel.Text,
                Customer = "Stanley"
            };

            foreach (DataGridViewRow row in dataGrid.Rows)
            {
                if (row.IsNewRow) continue;
                if (IsEmptyRow(row)) continue;

                var item = new LoadItem
                {
                    Date = ReadDate(row.Cells["clmDate"].Value),
                    SourceInvoiceNumber = row.Cells["clmInvoiceNum"].Value?.ToString()?.Trim() ?? string.Empty,
                    Description = row.Cells["clmDescription"].Value?.ToString() ?? string.Empty,
                    Weight = ReadDecimal(row.Cells["clmWeight"].Value),
                    Quantity = ReadInt(row.Cells["clmQuantity"].Value),
                    UnitPrice = ReadDecimal(row.Cells["clmPrice"].Value),
                    CalcType = cboCalcType.SelectedItem?.ToString() ?? "Weight"
                };

                invoice.Loads.Add(item);
            }

            if (validateSourceInvoiceNumbers)
            {
                ValidateSourceInvoiceNumbers(invoice);
            }

            _invoiceService.CalculateTotals(invoice);
            return invoice;
        }

        private void ConfigureGrid()
        {
            cboCalcType.SelectedIndex = 0;
            UseDatePickerColumn();
            clmInvoiceNum.ReadOnly = false;
            dataGrid.AllowUserToAddRows = true;
            dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            btnGenerate.AutoSize = true;
            btnCalculate.AutoSize = true;
            btnAddRow.AutoSize = true;
            ConfigureStatementButton();
            ConfigureSettingsButton();
        }

        private void ApplyVisualStyle()
        {
            AppStyles.ApplyForm(this);
            Text = "Faith Trucking Invoice Builder";
            MinimumSize = new Size(920, 620);

            AppStyles.ApplyHeaderPanel(tableLayoutHeader);
            FixHeaderSpacing();
            tableLayoutPanel1.BackColor = AppStyles.Panel;
            tableLayoutPanel1.Padding = new Padding(12);
            tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            lblInvoiceNum.ForeColor = AppStyles.Primary;
            lblInvoiceNum.Font = new Font("Segoe UI Semibold", 12F);

            AppStyles.ApplyTotalLabel(lblSubTotal);
            AppStyles.ApplyTotalLabel(lblVAT);
            AppStyles.ApplyTotalLabel(lblTotal, emphasized: true);

            AppStyles.ApplyButton(btnGenerate, ButtonRole.Primary);
            AppStyles.ApplyButton(btnCalculate, ButtonRole.Secondary);
            AppStyles.ApplyButton(btnAddRow, ButtonRole.Secondary);
            AppStyles.ApplyButton(_btnGenerateStatement, ButtonRole.Accent);
            AppStyles.ApplyButton(_btnSettings, ButtonRole.Secondary);
        }

        private void FixHeaderSpacing()
        {
            TableLayoutMain.RowStyles[0] = new RowStyle(SizeType.Absolute, 92F);
            tableLayoutHeader.MinimumSize = new Size(0, 84);
            tableLayoutHeader.RowStyles.Clear();
            tableLayoutHeader.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutHeader.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));

            foreach (var label in new[] { lblInvoiceNumHeader, lblDateHeader, lblVesselHeader, lblCalcTypeHeader, lblInvoiceNum })
            {
                label.AutoSize = false;
                label.TextAlign = ContentAlignment.MiddleLeft;
                label.Margin = new Padding(4, 0, 4, 3);
            }

            lblInvoiceNum.Margin = new Padding(4, 2, 4, 2);
            txtVessel.Margin = new Padding(4, 2, 4, 4);
            dtpDate.Margin = new Padding(4, 2, 4, 4);
            cboCalcType.Margin = new Padding(4, 2, 4, 4);
            dtpDate.Dock = DockStyle.Fill;
            cboCalcType.Dock = DockStyle.Fill;
            txtVessel.Dock = DockStyle.Fill;
        }

        private void ConfigureStatementButton()
        {
            _btnGenerateStatement.Text = "Generate Statement";
            _btnGenerateStatement.AutoSize = true;
            _btnGenerateStatement.Height = btnGenerate.Height;
            _btnGenerateStatement.Click += (_, _) => OpenStatementBuilder();

            if (!flowLayoutPanel1.Controls.Contains(_btnGenerateStatement))
            {
                flowLayoutPanel1.Controls.Add(_btnGenerateStatement);
            }

            AppStyles.ApplyButton(_btnGenerateStatement, ButtonRole.Accent);
        }

        private void ConfigureSettingsButton()
        {
            _btnSettings.Text = "Settings";
            _btnSettings.AutoSize = true;
            _btnSettings.Height = btnGenerate.Height;
            _btnSettings.Click += (_, _) => OpenSettings();

            if (!flowLayoutPanel1.Controls.Contains(_btnSettings))
            {
                flowLayoutPanel1.Controls.Add(_btnSettings);
            }

            AppStyles.ApplyButton(_btnSettings, ButtonRole.Secondary);
        }

        private void WireEvents()
        {
            btnAddRow.Click += (_, _) => AddLoadRow();
            btnCalculate.Click += (_, _) => CalculateAndDisplayTotals();
            btnGenerate.Click += (_, _) => GenerateInvoicePdf();
            cboCalcType.SelectedIndexChanged += (_, _) => CalculateAndDisplayTotals();
            dataGrid.CellEndEdit += (_, _) => CalculateAndDisplayTotals();
            dataGrid.RowsAdded += (_, _) => NumberRows();
            dataGrid.RowsRemoved += (_, _) =>
            {
                NumberRows();
                CalculateAndDisplayTotals();
            };
        }

        private void StartNewInvoice()
        {
            lblInvoiceNum.Text = _invoiceNumberService.GetNextInvoiceNumberPreview().ToString(CultureInfo.InvariantCulture);
            dtpDate.Value = DateTime.Today;
            lblSubTotal.Text = FormatCurrency(0);
            lblVAT.Text = FormatCurrency(0);
            lblTotal.Text = FormatCurrency(0);
            cboCalcType.SelectedIndex = 0;
            AddLoadRow();
        }

        private void AddLoadRow()
        {
            var previousRow = dataGrid.Rows
                .Cast<DataGridViewRow>()
                .Where(row => !row.IsNewRow)
                .LastOrDefault();

            var rowIndex = dataGrid.Rows.Add();
            var row = dataGrid.Rows[rowIndex];
            row.Cells["clmNum"].Value = rowIndex + 1;
            row.Cells["clmDate"].Value = DateTime.Today;
            row.Cells["clmDescription"].Value = previousRow?.Cells["clmDescription"].Value;
            row.Cells["clmQuantity"].Value = previousRow?.Cells["clmQuantity"].Value ?? 1;
            row.Cells["clmPrice"].Value = previousRow?.Cells["clmPrice"].Value;
            NumberRows();
        }

        private void UseDatePickerColumn()
        {
            var dateColumn = dataGrid.Columns["clmDate"];
            if (dateColumn is CalendarColumn)
            {
                return;
            }

            var columnIndex = dateColumn.Index;
            dataGrid.Columns.Remove(dateColumn);
            dataGrid.Columns.Insert(columnIndex, new CalendarColumn
            {
                FillWeight = 120F,
                HeaderText = "DATE",
                Name = "clmDate"
            });
        }

        private void NumberRows()
        {
            var number = 1;
            foreach (DataGridViewRow row in dataGrid.Rows)
            {
                if (row.IsNewRow) continue;
                row.Cells["clmNum"].Value = number++;
            }
        }

        private void CalculateAndDisplayTotals()
        {
            try
            {
                var invoice = BuildInvoiceFromUI();

                var loadIndex = 0;
                foreach (DataGridViewRow row in dataGrid.Rows)
                {
                    if (row.IsNewRow || IsEmptyRow(row)) continue;

                    var item = invoice.Loads.ElementAtOrDefault(loadIndex);
                    if (item is not null)
                    {
                        row.Cells["clmAmount"].Value = item.Amount.ToString("N2", CultureInfo.InvariantCulture);
                    }

                    loadIndex++;
                }

                lblSubTotal.Text = FormatCurrency(invoice.Subtotal);
                lblVAT.Text = FormatCurrency(invoice.Vat);
                lblTotal.Text = FormatCurrency(invoice.Total);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Please check the load details. {ex.Message}", "Calculation error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void GenerateInvoicePdf()
        {
            try
            {
                var invoice = BuildInvoiceFromUI(validateSourceInvoiceNumbers: true);
                if (invoice.Loads.Count == 0)
                {
                    MessageBox.Show("Add at least one load before generating the invoice.", "Invoice Builder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SortLoadsByInvoiceNumber(invoice);

                var fullPath = BuildInvoicePath(invoice);

                using var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = Path.GetFileName(fullPath),
                    InitialDirectory = Path.GetDirectoryName(fullPath),
                    Title = "Save invoice PDF"
                };

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return;

                _pdfInvoiceService.SaveInvoice(invoice, saveDialog.FileName);
                _invoiceNumberService.MarkInvoiceNumberUsed(invoice.InvoiceNumber);
                _generatedInvoiceRepository.Save(BuildGeneratedInvoiceRecord(invoice, saveDialog.FileName));

                MessageBox.Show("Invoice PDF generated successfully.", "Invoice Builder", MessageBoxButtons.OK, MessageBoxIcon.Information);

                dataGrid.Rows.Clear();
                txtVessel.Clear();
                StartNewInvoice();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not generate the invoice. {ex.Message}", "Invoice Builder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenStatementBuilder()
        {
            using var statementBuilder = new StatementBuilder();
            statementBuilder.ShowDialog(this);
        }

        private void OpenSettings()
        {
            if (!SettingsForm.PromptForAdmin(this))
            {
                MessageBox.Show("Admin password was not accepted.", "Admin Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var settingsForm = new SettingsForm();
            settingsForm.ShowDialog(this);
        }

        private GeneratedInvoiceRecord BuildGeneratedInvoiceRecord(Invoice invoice, string pdfPath)
        {
            var settings = _settingsService.Load();

            return new GeneratedInvoiceRecord
            {
                InvoiceNumber = invoice.InvoiceNumber,
                Date = invoice.Date,
                Customer = invoice.Customer,
                Vessel = invoice.Vessel,
                VehicleRegistration = settings.DefaultVehicleRegistration,
                Description = BuildStatementDescription(invoice),
                LoadCount = invoice.Loads.Count,
                StatementDescription = BuildStatementDescriptionDetail(invoice),
                Subtotal = invoice.Subtotal,
                Vat = invoice.Vat,
                Total = invoice.Total,
                PdfPath = pdfPath,
                GeneratedAt = DateTime.Now
            };
        }

        private static string BuildStatementDescription(Invoice invoice)
        {
            var loadCount = invoice.Loads.Count;
            var detail = BuildStatementDescriptionDetail(invoice);

            if (loadCount == 0)
            {
                return detail;
            }

            if (string.IsNullOrWhiteSpace(detail))
            {
                return loadCount == 1 ? "1 X LOAD" : $"{loadCount} X LOADS";
            }

            return loadCount == 1 ? $"1 X LOAD {detail}" : $"{loadCount} X LOADS {detail}";
        }

        private static string BuildStatementDescriptionDetail(Invoice invoice)
        {
            var firstDescription = invoice.Loads
                .Select(load => load.Description)
                .FirstOrDefault(description => !string.IsNullOrWhiteSpace(description))
                ?.Trim();

            return firstDescription ?? string.Empty;
        }
        private string BuildInvoicePath(Invoice invoice)
        {
            var year = invoice.Date.Year;
            var monthName = invoice.Date.ToString("MMMM"); // April
            var monthYear = $"{monthName} {year}";

            // Base folder (you can change this if needed)
            var baseFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "Faith Trucking",
                "Books",
                year.ToString(),
                monthYear,
                "Invoices"
            );

            // Ensure directories exist
            Directory.CreateDirectory(baseFolder);

            // Clean vessel name (important for file safety)
            var vessel = SanitizeFileName(invoice.Vessel);

            var fileName = $"Invoice {monthName} {year} {vessel}.pdf";

            return Path.Combine(baseFolder, fileName);
        }
        private string SanitizeFileName(string input)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                input = input.Replace(c.ToString(), "");

            return input.Trim();
        }
        private static bool IsEmptyRow(DataGridViewRow row)
        {
            return string.IsNullOrWhiteSpace(row.Cells["clmDescription"].Value?.ToString())
                && string.IsNullOrWhiteSpace(row.Cells["clmInvoiceNum"].Value?.ToString())
                && row.Cells["clmWeight"].Value is null
                && row.Cells["clmPrice"].Value is null;
        }

        private static void ValidateSourceInvoiceNumbers(Invoice invoice)
        {
            var seenNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var load in invoice.Loads)
            {
                if (string.IsNullOrWhiteSpace(load.SourceInvoiceNumber))
                {
                    throw new InvalidOperationException("Each row must have its own invoice number in the INVOICE No. column.");
                }

                if (!seenNumbers.Add(load.SourceInvoiceNumber))
                {
                    throw new InvalidOperationException($"Invoice number {load.SourceInvoiceNumber} is duplicated in this document.");
                }
            }
        }

        private static void SortLoadsByInvoiceNumber(Invoice invoice)
        {
            invoice.Loads = invoice.Loads
                .OrderBy(load => ReadInvoiceNumberSortValue(load.SourceInvoiceNumber))
                .ThenBy(load => load.SourceInvoiceNumber, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static int ReadInvoiceNumberSortValue(string invoiceNumber)
        {
            return int.TryParse(invoiceNumber, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
                ? number
                : int.MaxValue;
        }

        private static DateTime ReadDate(object? value)
        {
            if (value is DateTime date)
            {
                return date;
            }

            return DateTime.TryParse(value?.ToString(), out var parsedDate) ? parsedDate : DateTime.Today;
        }

        private static decimal ReadDecimal(object? value)
        {
            if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return 0;
            }

            return decimal.Parse(value.ToString()!, CultureInfo.CurrentCulture);
        }

        private static int ReadInt(object? value)
        {
            if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return 1;
            }

            return int.Parse(value.ToString()!, CultureInfo.CurrentCulture);
        }

        private static string FormatCurrency(decimal value)
        {
            return $"R {value:N2}";
        }

        private void dataGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {

        }
    }
}
