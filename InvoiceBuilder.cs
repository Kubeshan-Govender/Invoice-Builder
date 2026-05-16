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
        private readonly InvoiceDraftService _draftService = new();
        private readonly Button _btnGenerateStatement = new();
        private readonly Button _btnSettings = new();
        private readonly Button _btnDeleteRow = new();
        private bool _isLoadingDraft;
        private bool _isUpdatingTotals;

        public InvoiceBuilder()
        {
            InitializeComponent();
            ConfigureGrid();
            ApplyVisualStyle();
            WireEvents();
            LoadDraftOrStartNewInvoice();
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
                    CalcType = ReadRowCalcType(row)
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
            EnsureCalculationColumn();
            clmInvoiceNum.ReadOnly = false;
            dataGrid.AllowUserToAddRows = false;
            dataGrid.AllowUserToResizeColumns = false;
            dataGrid.AllowUserToResizeRows = false;
            dataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGrid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            foreach (DataGridViewColumn column in dataGrid.Columns)
            {
                column.Resizable = DataGridViewTriState.False;
            }

            btnGenerate.AutoSize = true;
            btnCalculate.AutoSize = true;
            btnAddRow.AutoSize = true;
            ConfigureStatementButton();
            ConfigureSettingsButton();
            ConfigureDeleteRowButton();
        }

        private void ApplyVisualStyle()
        {
            AppStyles.ApplyForm(this);
            Text = "Faith Trucking Invoice Builder";
            MinimumSize = new Size(920, 620);

            AppStyles.ApplyHeaderPanel(tableLayoutHeader);
            FixHeaderSpacing();
            ConfigureTotalsPanel();

            lblInvoiceNum.ForeColor = AppStyles.Primary;
            lblInvoiceNum.Font = new Font("Segoe UI Semibold", 12F);

            AppStyles.ApplyTotalLabel(lblSubTotal);
            AppStyles.ApplyTotalLabel(lblVAT);
            AppStyles.ApplyTotalLabel(lblTotal, emphasized: true);

            AppStyles.ApplyButton(btnGenerate, ButtonRole.Primary);
            AppStyles.ApplyButton(btnCalculate, ButtonRole.Secondary);
            AppStyles.ApplyButton(btnAddRow, ButtonRole.Secondary);
            AppStyles.ApplyButton(_btnDeleteRow, ButtonRole.Secondary);
            AppStyles.ApplyButton(_btnGenerateStatement, ButtonRole.Accent);
            AppStyles.ApplyButton(_btnSettings, ButtonRole.Secondary);
            AppStyles.ApplyButtonIcon(btnGenerate, ButtonIcon.Generate, ButtonRole.Primary);
            AppStyles.ApplyButtonIcon(btnCalculate, ButtonIcon.Calculate, ButtonRole.Secondary);
            AppStyles.ApplyButtonIcon(btnAddRow, ButtonIcon.Add, ButtonRole.Secondary);
            AppStyles.ApplyButtonIcon(_btnDeleteRow, ButtonIcon.Delete, ButtonRole.Secondary);
            AppStyles.ApplyButtonIcon(_btnGenerateStatement, ButtonIcon.Statement, ButtonRole.Accent);
            AppStyles.ApplyButtonIcon(_btnSettings, ButtonIcon.Settings, ButtonRole.Secondary);
        }

        private void ConfigureTotalsPanel()
        {
            tableLayoutPanel1.BackColor = AppStyles.Panel;
            tableLayoutPanel1.Padding = new Padding(18, 12, 18, 12);
            tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel1.Size = new Size(360, 132);
            tableLayoutPanel1.MinimumSize = new Size(360, 132);
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));

            foreach (var label in new[] { lblSubTotalHeader, lblVATHeader, lblTotalHeader })
            {
                label.AutoSize = false;
                label.Dock = DockStyle.Fill;
                label.TextAlign = ContentAlignment.MiddleLeft;
                label.Margin = new Padding(4, 0, 8, 0);
            }

            foreach (var label in new[] { lblSubTotal, lblVAT, lblTotal })
            {
                label.AutoSize = false;
                label.Dock = DockStyle.Fill;
                label.TextAlign = ContentAlignment.MiddleRight;
                label.Margin = new Padding(8, 0, 4, 0);
            }
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

        private void ConfigureDeleteRowButton()
        {
            _btnDeleteRow.Text = "Delete Row";
            _btnDeleteRow.AutoSize = true;
            _btnDeleteRow.Height = btnGenerate.Height;
            _btnDeleteRow.Click += (_, _) => DeleteCurrentRow();

            if (!flowLayoutPanel1.Controls.Contains(_btnDeleteRow))
            {
                flowLayoutPanel1.Controls.Add(_btnDeleteRow);
            }

            AppStyles.ApplyButton(_btnDeleteRow, ButtonRole.Secondary);
        }

        private void WireEvents()
        {
            btnAddRow.Click += (_, _) => AddLoadRowAndFocusDate();
            btnCalculate.Click += (_, _) => CalculateAndDisplayTotals();
            btnGenerate.Click += (_, _) => GenerateInvoicePdf();
            AcceptButton = btnAddRow;
            KeyPreview = true;
            KeyDown += InvoiceBuilder_KeyDown;
            FormClosing += (_, _) =>
            {
                CommitGridEdits();
                SaveCurrentDraft();
            };
            txtVessel.TextChanged += (_, _) => SaveCurrentDraft();
            dtpDate.ValueChanged += (_, _) => SaveCurrentDraft();
            cboCalcType.SelectedIndexChanged += (_, _) =>
            {
                CalculateAndDisplayTotals();
                SaveCurrentDraft();
            };
            dataGrid.KeyDown += DataGrid_KeyDown;
            dataGrid.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (dataGrid.IsCurrentCellDirty)
                {
                    dataGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };
            dataGrid.CellValueChanged += (_, e) =>
            {
                if (!_isUpdatingTotals && e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    var columnName = dataGrid.Columns[e.ColumnIndex].Name;
                    if (columnName == "clmCalcType")
                    {
                        CalculateAndDisplayTotals();
                    }
                }

                SaveCurrentDraft();
            };
            dataGrid.CellEndEdit += (_, _) =>
            {
                CalculateAndDisplayTotals();
                SaveCurrentDraft();
            };
            dataGrid.RowsAdded += (_, _) =>
            {
                NumberRows();
                SaveCurrentDraft();
            };
            dataGrid.RowsRemoved += (_, _) =>
            {
                NumberRows();
                CalculateAndDisplayTotals();
                SaveCurrentDraft();
            };
        }

        private void LoadDraftOrStartNewInvoice()
        {
            var draft = _draftService.Load();
            if (draft is null)
            {
                StartNewInvoice();
                return;
            }

            LoadInvoiceDraft(draft);
        }

        private void StartNewInvoice()
        {
            _isLoadingDraft = true;
            dataGrid.Rows.Clear();
            lblInvoiceNum.Text = _invoiceNumberService.GetNextInvoiceNumberPreview().ToString(CultureInfo.InvariantCulture);
            dtpDate.Value = DateTime.Today;
            txtVessel.Clear();
            lblSubTotal.Text = FormatCurrency(0);
            lblVAT.Text = FormatCurrency(0);
            lblTotal.Text = FormatCurrency(0);
            cboCalcType.SelectedIndex = 0;
            AddLoadRow();
            _isLoadingDraft = false;
            SaveCurrentDraft();
        }

        private int AddLoadRow()
        {
            var previousRow = dataGrid.Rows
                .Cast<DataGridViewRow>()
                .Where(row => !row.IsNewRow)
                .LastOrDefault();

            var rowIndex = dataGrid.Rows.Add();
            var row = dataGrid.Rows[rowIndex];
            row.Cells["clmNum"].Value = rowIndex + 1;
            row.Cells["clmDate"].Value = ReadDate(previousRow?.Cells["clmDate"].Value);
            row.Cells["clmCalcType"].Value = GetDefaultCalcType();
            row.Cells["clmDescription"].Value = previousRow?.Cells["clmDescription"].Value;
            row.Cells["clmQuantity"].Value = previousRow?.Cells["clmQuantity"].Value ?? 1;
            row.Cells["clmPrice"].Value = previousRow?.Cells["clmPrice"].Value;
            NumberRows();
            SaveCurrentDraft();
            return rowIndex;
        }

        private void AddLoadRowAndFocusDate()
        {
            CommitGridEdits();
            var rowIndex = AddLoadRow();
            FocusRowDateCell(rowIndex);
        }

        private void FocusRowDateCell(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dataGrid.Rows.Count)
            {
                return;
            }

            dataGrid.CurrentCell = dataGrid.Rows[rowIndex].Cells["clmDate"];
            dataGrid.BeginEdit(true);
        }

        private void DeleteCurrentRow()
        {
            var row = dataGrid.CurrentRow;
            if (row is null || row.IsNewRow)
            {
                MessageBox.Show("Select a load row to delete.", "Invoice Builder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show("Delete the selected load row?", "Invoice Builder", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            dataGrid.Rows.Remove(row);
            if (dataGrid.Rows.Count == 0)
            {
                AddLoadRow();
            }

            NumberRows();
            CalculateAndDisplayTotals();
            SaveCurrentDraft();
        }

        private void DataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            AddLoadRowAndFocusDate();
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void InvoiceBuilder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && ActiveControl != dataGrid)
            {
                AddLoadRowAndFocusDate();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void LoadInvoiceDraft(Invoice draft)
        {
            _isLoadingDraft = true;
            dataGrid.Rows.Clear();
            lblInvoiceNum.Text = draft.InvoiceNumber.ToString(CultureInfo.InvariantCulture);
            dtpDate.Value = draft.Date == default ? DateTime.Today : draft.Date;
            txtVessel.Text = draft.Vessel;

            var calcType = draft.Loads.FirstOrDefault()?.CalcType ?? "Weight";
            cboCalcType.SelectedItem = cboCalcType.Items.Contains(calcType) ? calcType : "Weight";

            foreach (var load in draft.Loads)
            {
                var rowIndex = dataGrid.Rows.Add();
                var row = dataGrid.Rows[rowIndex];
                row.Cells["clmDate"].Value = load.Date == default ? DateTime.Today : load.Date;
                row.Cells["clmCalcType"].Value = string.IsNullOrWhiteSpace(load.CalcType) ? GetDefaultCalcType() : load.CalcType;
                row.Cells["clmInvoiceNum"].Value = load.SourceInvoiceNumber;
                row.Cells["clmDescription"].Value = load.Description;
                row.Cells["clmWeight"].Value = load.Weight == 0 ? null : load.Weight;
                row.Cells["clmQuantity"].Value = load.Quantity == 0 ? 1 : load.Quantity;
                row.Cells["clmPrice"].Value = load.UnitPrice == 0 ? null : load.UnitPrice;
                row.Cells["clmAmount"].Value = load.Amount == 0 ? null : load.Amount.ToString("N2", CultureInfo.InvariantCulture);
            }

            if (dataGrid.Rows.Count == 0)
            {
                AddLoadRow();
            }

            NumberRows();
            _isLoadingDraft = false;
            CalculateAndDisplayTotals();
            SaveCurrentDraft();
        }

        private void SaveCurrentDraft()
        {
            if (_isLoadingDraft)
            {
                return;
            }

            try
            {
                _draftService.Save(BuildInvoiceFromUI());
            }
            catch
            {
                // Draft saving should never block the user from finishing the invoice.
            }
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

        private void EnsureCalculationColumn()
        {
            if (dataGrid.Columns.Contains("clmCalcType"))
            {
                return;
            }

            var descriptionIndex = dataGrid.Columns["clmDescription"].Index;
            dataGrid.Columns.Insert(descriptionIndex, new DataGridViewComboBoxColumn
            {
                DataSource = new[] { "Weight", "Flat" },
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                FillWeight = 95F,
                HeaderText = "CALC",
                Name = "clmCalcType",
                Resizable = DataGridViewTriState.False
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
                CommitGridEdits();
                _isUpdatingTotals = true;
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
                _isUpdatingTotals = false;
            }
            catch (Exception ex)
            {
                _isUpdatingTotals = false;
                MessageBox.Show($"Please check the load details. {ex.Message}", "Calculation error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void GenerateInvoicePdf()
        {
            try
            {
                CommitGridEdits();
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
                _draftService.Clear();

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

        private string GetDefaultCalcType()
        {
            return cboCalcType.SelectedItem?.ToString() ?? "Weight";
        }

        private string ReadRowCalcType(DataGridViewRow row)
        {
            if (dataGrid.Columns.Contains("clmCalcType"))
            {
                var value = row.Cells["clmCalcType"].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return GetDefaultCalcType();
        }

        private void CommitGridEdits()
        {
            if (dataGrid.IsCurrentCellDirty)
            {
                dataGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }

            dataGrid.EndEdit(DataGridViewDataErrorContexts.Commit);
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
