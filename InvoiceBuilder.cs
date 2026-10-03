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
using System.Threading.Tasks;
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
        private readonly Button _btnCheckGrvs = new();
        private readonly Button _btnOrderGrvs = new();
        private readonly Label _lblCargoTypeHeader = new();
        private readonly ComboBox _cboCargoType = new();
        private readonly FlowLayoutPanel _headerActionsPanel = new();
        private readonly Services.CargoSpreadsheetService _cargoSpreadsheetService = new();
        private bool _isLoadingDraft;
        private bool _isUpdatingTotals;
        private bool _duplicateCheckPerformed;
        private DateTime _invoiceDate = DateTime.Today;
        private string _vesselName = string.Empty;

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
                CargoType = _cboCargoType.SelectedItem?.ToString() ?? CargoTypes.Spodumene,
                Date = _invoiceDate,
                Vessel = _vesselName,
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
                    CargoReference = row.Cells["clmCargoReference"].Value?.ToString()?.Trim() ?? string.Empty,
                    SerialBagNumber = row.Cells["clmSerialBag"].Value?.ToString()?.Trim() ?? string.Empty,
                    Weight = ReadDecimal(row.Cells["clmWeight"].Value),
                    Quantity = ReadInt(row.Cells["clmQuantity"].Value),
                    UnitPrice = ReadDecimal(row.Cells["clmPrice"].Value),
                    CalcType = ReadRowCalcType(row),
                    FullEmpty = row.Cells["clmFullEmpty"].Value?.ToString() ?? string.Empty
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
            cboCalcType.SelectedItem = _cboCargoType.SelectedItem?.ToString() == CargoTypes.Containers ? "Flat" : "Weight";
            UseDatePickerColumn();
            EnsureCalculationColumn();
            EnsureFullEmptyColumn();
            EnsureSerialBagColumn();
            EnsureCargoReferenceColumn();
            ConfigureCargoTypeSelector();
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
            UpdateCargoColumnLabels();

            btnGenerate.AutoSize = true;
            btnCalculate.AutoSize = true;
            btnAddRow.AutoSize = true;
            ConfigureGenerateInvoiceButton();
            ConfigureStatementButton();
            ConfigureSettingsButton();
            ConfigureDeleteRowButton();
            ConfigureGrvButtons();
        }

        private void ConfigureCargoTypeSelector()
        {
            tableLayoutHeader.SuspendLayout();
            tableLayoutHeader.Controls.Clear();
            tableLayoutHeader.ColumnCount = 4;
            tableLayoutHeader.ColumnStyles.Clear();
            tableLayoutHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            tableLayoutHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            tableLayoutHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44F));
            tableLayoutHeader.RowCount = 2;

            _lblCargoTypeHeader.Text = "Cargo Type:";
            _lblCargoTypeHeader.Dock = DockStyle.Fill;
            _lblCargoTypeHeader.TextAlign = ContentAlignment.MiddleLeft;
            _lblCargoTypeHeader.Margin = new Padding(4, 0, 4, 3);

            _cboCargoType.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboCargoType.Dock = DockStyle.Fill;
            _cboCargoType.Items.AddRange(CargoTypes.All.Cast<object>().ToArray());
            _cboCargoType.SelectedItem = CargoTypes.Spodumene;

            _headerActionsPanel.Dock = DockStyle.Fill;
            _headerActionsPanel.FlowDirection = FlowDirection.RightToLeft;
            _headerActionsPanel.WrapContents = false;
            _headerActionsPanel.Padding = new Padding(4);

            lblInvoiceNumHeader.Text = "Invoice Number:";
            lblCalcTypeHeader.Text = "Calculation:";
            tableLayoutHeader.Controls.Add(lblInvoiceNumHeader, 0, 0);
            tableLayoutHeader.Controls.Add(lblInvoiceNum, 0, 1);
            tableLayoutHeader.Controls.Add(lblCalcTypeHeader, 1, 0);
            tableLayoutHeader.Controls.Add(cboCalcType, 1, 1);
            tableLayoutHeader.Controls.Add(_lblCargoTypeHeader, 2, 0);
            tableLayoutHeader.Controls.Add(_cboCargoType, 2, 1);
            tableLayoutHeader.Controls.Add(_headerActionsPanel, 3, 0);
            tableLayoutHeader.SetRowSpan(_headerActionsPanel, 2);
            tableLayoutHeader.ResumeLayout(true);
        }

        private void EnsureFullEmptyColumn()
        {
            if (dataGrid.Columns.Contains("clmFullEmpty"))
            {
                return;
            }

            dataGrid.Columns.Add(new DataGridViewComboBoxColumn
            {
                DataSource = new[] { "Full", "Empty" },
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                FillWeight = 105F,
                HeaderText = "FULL/EMPTY",
                Name = "clmFullEmpty",
                Resizable = DataGridViewTriState.False,
                Visible = false
            });
        }

        private void UpdateCargoColumnLabels()
        {
            var cargoType = _cboCargoType.SelectedItem?.ToString() ?? CargoTypes.Spodumene;
            dataGrid.Columns["clmDescription"].HeaderText = "DESCRIPTION";
            dataGrid.Columns["clmCargoReference"].HeaderText = cargoType switch
            {
                CargoTypes.Containers => "CONTAINER NUMBER",
                CargoTypes.BreakBulk => "PRODUCT",
                _ => "WEIGHBRIDGE TICKET"
            };
            dataGrid.Columns["clmWeight"].HeaderText = cargoType == CargoTypes.Containers ? "GROSS WEIGHT" : "WEIGHT";
            dataGrid.Columns["clmPrice"].HeaderText = "RATE";
            dataGrid.Columns["clmPrice"].ToolTipText = cargoType == CargoTypes.Spodumene
                ? "The Spodumene template has no Rate column; this rate is used to calculate the invoice PDF."
                : "This rate is written to the cargo spreadsheet and used to calculate the invoice PDF.";
            dataGrid.Columns["clmFullEmpty"].Visible = cargoType == CargoTypes.Containers;
            dataGrid.Columns["clmSerialBag"].Visible = cargoType == CargoTypes.BreakBulk;
        }

        private void EnsureSerialBagColumn()
        {
            if (dataGrid.Columns.Contains("clmSerialBag"))
            {
                return;
            }

            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                FillWeight = 115F,
                HeaderText = "SERIAL/BAG NR",
                Name = "clmSerialBag",
                Resizable = DataGridViewTriState.False,
                Visible = false
            });
        }

        private void EnsureCargoReferenceColumn()
        {
            if (dataGrid.Columns.Contains("clmCargoReference"))
            {
                return;
            }

            var descriptionIndex = dataGrid.Columns["clmDescription"].Index;
            dataGrid.Columns.Insert(descriptionIndex + 1, new DataGridViewTextBoxColumn
            {
                FillWeight = 185F,
                HeaderText = "WEIGHBRIDGE TICKET",
                Name = "clmCargoReference",
                Resizable = DataGridViewTriState.False
            });
        }

        private void ApplyVisualStyle()
        {
            AppStyles.ApplyForm(this);
            Text = "Faith Trucking Invoice Builder";
            MinimumSize = new Size(1150, 620);
            Width = Math.Max(Width, 1150);

            AppStyles.ApplyHeaderPanel(tableLayoutHeader);
            FixHeaderSpacing();
            ConfigureTotalsPanel();

            lblInvoiceNum.ForeColor = AppStyles.Primary;
            lblInvoiceNum.Font = new Font("Segoe UI Semibold", 12F);

            AppStyles.ApplyTotalLabel(lblSubTotal);
            AppStyles.ApplyTotalLabel(lblVAT);
            AppStyles.ApplyTotalLabel(lblTotal, emphasized: true);

            AppStyles.ApplyButton(btnCalculate, ButtonRole.Secondary);
            AppStyles.ApplyButton(btnAddRow, ButtonRole.Secondary);
            AppStyles.ApplyButton(_btnDeleteRow, ButtonRole.Secondary);
            AppStyles.ApplyButton(_btnCheckGrvs, ButtonRole.Secondary);
            AppStyles.ApplyButton(_btnOrderGrvs, ButtonRole.Secondary);
            AppStyles.ApplyButton(_btnGenerateStatement, ButtonRole.Accent);
            AppStyles.ApplyButton(_btnSettings, ButtonRole.Secondary);
            AppStyles.ApplyButtonIcon(btnCalculate, ButtonIcon.Calculate, ButtonRole.Secondary);
            AppStyles.ApplyButtonIcon(btnAddRow, ButtonIcon.Add, ButtonRole.Secondary);
            AppStyles.ApplyButtonIcon(_btnDeleteRow, ButtonIcon.Delete, ButtonRole.Secondary);
            AppStyles.ApplyButtonIcon(_btnGenerateStatement, ButtonIcon.Statement, ButtonRole.Accent);
            AppStyles.ApplyButtonIcon(_btnSettings, ButtonIcon.Settings, ButtonRole.Secondary);

            AppStyles.ApplyControl(_lblCargoTypeHeader);
            AppStyles.ApplyControl(_cboCargoType);
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

            foreach (var label in new[] { lblInvoiceNumHeader, lblCalcTypeHeader, lblInvoiceNum, _lblCargoTypeHeader })
            {
                label.AutoSize = false;
                label.TextAlign = ContentAlignment.MiddleLeft;
                label.Margin = new Padding(4, 0, 4, 3);
            }

            lblInvoiceNum.Margin = new Padding(4, 2, 4, 2);
            cboCalcType.Margin = new Padding(4, 2, 4, 4);
            _cboCargoType.Margin = new Padding(4, 2, 4, 4);
            cboCalcType.Dock = DockStyle.Fill;
            _cboCargoType.Dock = DockStyle.Fill;
        }

        private void ConfigureStatementButton()
        {
            _btnGenerateStatement.Text = "Generate Statement";
            _btnGenerateStatement.AutoSize = true;
            _btnGenerateStatement.Height = btnGenerate.Height;
            _btnGenerateStatement.Click += (_, _) => OpenStatementBuilder();

            if (!_headerActionsPanel.Controls.Contains(_btnGenerateStatement))
            {
                _headerActionsPanel.Controls.Add(_btnGenerateStatement);
            }

            AppStyles.ApplyButton(_btnGenerateStatement, ButtonRole.Accent);
        }

        private void ConfigureSettingsButton()
        {
            _btnSettings.Text = "Settings";
            _btnSettings.AutoSize = true;
            _btnSettings.Height = btnGenerate.Height;
            _btnSettings.Click += (_, _) => OpenSettings();

            if (!_headerActionsPanel.Controls.Contains(_btnSettings))
            {
                _headerActionsPanel.Controls.Add(_btnSettings);
            }

            AppStyles.ApplyButton(_btnSettings, ButtonRole.Secondary);
        }

        private void ConfigureGenerateInvoiceButton()
        {
            flowLayoutPanel1.Controls.Remove(btnGenerate);
            btnGenerate.Text = "Generate Invoice";
            btnGenerate.AutoSize = true;
            btnGenerate.Height = 38;
            btnGenerate.Click -= btnGenerate_Click;
            btnGenerate.Click += (_, _) => GenerateInvoicePdf();
            _headerActionsPanel.Controls.Add(btnGenerate);
            AppStyles.ApplyButton(btnGenerate, ButtonRole.Primary);
            AppStyles.ApplyButtonIcon(btnGenerate, ButtonIcon.Generate, ButtonRole.Primary);
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

        private void ConfigureGrvButtons()
        {
            _btnCheckGrvs.Text = "Check GRVs";
            _btnCheckGrvs.AutoSize = true;
            _btnCheckGrvs.Height = btnGenerate.Height;
            _btnCheckGrvs.Click += (_, _) => CheckDuplicateGrvs();
            flowLayoutPanel1.Controls.Add(_btnCheckGrvs);
            AppStyles.ApplyButton(_btnCheckGrvs, ButtonRole.Secondary);

            _btnOrderGrvs.Text = "Order GRVs";
            _btnOrderGrvs.AutoSize = true;
            _btnOrderGrvs.Height = btnGenerate.Height;
            _btnOrderGrvs.Click += (_, _) => OrderGrvsLowToHigh();
            flowLayoutPanel1.Controls.Add(_btnOrderGrvs);
            AppStyles.ApplyButton(_btnOrderGrvs, ButtonRole.Secondary);
        }

        private void CheckDuplicateGrvs()
        {
            CommitGridEdits();
            _duplicateCheckPerformed = true;
            var duplicates = UpdateDuplicateGrvHighlights();
            if (duplicates.Count == 0)
            {
                MessageBox.Show("No duplicate GRV numbers were found.", "Check GRVs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show(
                $"Highlighted rows with duplicate GRV numbers: {string.Join(", ", duplicates)}.\n\nUse Order GRVs to sort from low to high.",
                "Duplicate GRVs found",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private List<string> UpdateDuplicateGrvHighlights()
        {
            var duplicateGroups = dataGrid.Rows
                .Cast<DataGridViewRow>()
                .Where(row => !row.IsNewRow)
                .Select(row => new { Row = row, Grv = row.Cells["clmInvoiceNum"].Value?.ToString()?.Trim() ?? string.Empty })
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Grv))
                .GroupBy(entry => entry.Grv, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .ToList();

            foreach (DataGridViewRow row in dataGrid.Rows)
            {
                if (row.IsNewRow) continue;
                row.DefaultCellStyle.BackColor = dataGrid.DefaultCellStyle.BackColor;
                row.DefaultCellStyle.ForeColor = dataGrid.DefaultCellStyle.ForeColor;
            }

            foreach (var group in duplicateGroups)
            {
                foreach (var entry in group)
                {
                    entry.Row.DefaultCellStyle.BackColor = Color.MistyRose;
                    entry.Row.DefaultCellStyle.ForeColor = Color.DarkRed;
                }
            }

            return duplicateGroups.Select(group => group.Key).ToList();
        }

        private void OrderGrvsLowToHigh()
        {
            CommitGridEdits();
            dataGrid.Sort(new GrvRowComparer());
            NumberRows();
            CalculateAndDisplayTotals();
            SaveCurrentDraft();
            if (_duplicateCheckPerformed)
            {
                UpdateDuplicateGrvHighlights();
            }
        }

        private sealed class GrvRowComparer : System.Collections.IComparer
        {
            public int Compare(object x, object y)
            {
                var left = (x as DataGridViewRow)?.Cells["clmInvoiceNum"].Value?.ToString()?.Trim() ?? string.Empty;
                var right = (y as DataGridViewRow)?.Cells["clmInvoiceNum"].Value?.ToString()?.Trim() ?? string.Empty;
                var leftIsNumeric = decimal.TryParse(left, NumberStyles.Number, CultureInfo.InvariantCulture, out var leftNumber);
                var rightIsNumeric = decimal.TryParse(right, NumberStyles.Number, CultureInfo.InvariantCulture, out var rightNumber);

                if (leftIsNumeric && rightIsNumeric) return leftNumber.CompareTo(rightNumber);
                if (leftIsNumeric != rightIsNumeric) return leftIsNumeric ? -1 : 1;
                if (string.IsNullOrEmpty(left) != string.IsNullOrEmpty(right)) return string.IsNullOrEmpty(left) ? 1 : -1;
                return StringComparer.OrdinalIgnoreCase.Compare(left, right);
            }
        }

        private void WireEvents()
        {
            btnAddRow.Click += (_, _) => AddLoadRowAndFocusDate();
            btnCalculate.Click += (_, _) => CalculateAndDisplayTotals();
            AcceptButton = btnAddRow;
            KeyPreview = true;
            KeyDown += InvoiceBuilder_KeyDown;
            FormClosing += (_, _) =>
            {
                CommitGridEdits();
                SaveCurrentDraft();
            };
            cboCalcType.SelectedIndexChanged += (_, _) =>
            {
                CalculateAndDisplayTotals();
                SaveCurrentDraft();
            };
            _cboCargoType.SelectedIndexChanged += (_, _) =>
            {
                var defaultCalcType = _cboCargoType.SelectedItem?.ToString() == CargoTypes.Containers ? "Flat" : "Weight";
                cboCalcType.SelectedItem = defaultCalcType;
                foreach (DataGridViewRow row in dataGrid.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        row.Cells["clmCalcType"].Value = defaultCalcType;
                    }
                }

                UpdateCargoColumnLabels();
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
                if (_duplicateCheckPerformed && e.RowIndex >= 0)
                {
                    UpdateDuplicateGrvHighlights();
                }

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
            _invoiceDate = DateTime.Today;
            _vesselName = string.Empty;
            lblSubTotal.Text = FormatCurrency(0);
            lblVAT.Text = FormatCurrency(0);
            lblTotal.Text = FormatCurrency(0);
            cboCalcType.SelectedItem = _cboCargoType.SelectedItem?.ToString() == CargoTypes.Containers ? "Flat" : "Weight";
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
            row.Cells["clmCargoReference"].Value = _cboCargoType.SelectedItem?.ToString() == CargoTypes.BreakBulk
                ? previousRow?.Cells["clmCargoReference"].Value
                : string.Empty;
            row.Cells["clmQuantity"].Value = previousRow?.Cells["clmQuantity"].Value ?? 1;
            row.Cells["clmPrice"].Value = previousRow?.Cells["clmPrice"].Value;
            row.Cells["clmFullEmpty"].Value = null;
            row.Cells["clmSerialBag"].Value = string.Empty;
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
            _invoiceDate = draft.Date == default ? DateTime.Today : draft.Date;
            _vesselName = draft.Vessel?.Trim() ?? string.Empty;
            _cboCargoType.SelectedItem = CargoTypes.All.Contains(draft.CargoType) ? draft.CargoType : CargoTypes.Spodumene;
            UpdateCargoColumnLabels();

            var defaultCalcType = draft.CargoType == CargoTypes.Containers ? "Flat" : "Weight";
            var calcType = draft.Loads.FirstOrDefault()?.CalcType ?? defaultCalcType;
            cboCalcType.SelectedItem = cboCalcType.Items.Contains(calcType) ? calcType : defaultCalcType;

            foreach (var load in draft.Loads)
            {
                var rowIndex = dataGrid.Rows.Add();
                var row = dataGrid.Rows[rowIndex];
                row.Cells["clmDate"].Value = load.Date == default ? DateTime.Today : load.Date;
                row.Cells["clmCalcType"].Value = string.IsNullOrWhiteSpace(load.CalcType) ? GetDefaultCalcType() : load.CalcType;
                row.Cells["clmInvoiceNum"].Value = load.SourceInvoiceNumber;
                row.Cells["clmDescription"].Value = load.Description;
                row.Cells["clmCargoReference"].Value = load.CargoReference;
                row.Cells["clmWeight"].Value = load.Weight == 0 ? null : load.Weight;
                row.Cells["clmQuantity"].Value = load.Quantity == 0 ? 1 : load.Quantity;
                row.Cells["clmPrice"].Value = load.UnitPrice == 0 ? null : load.UnitPrice;
                row.Cells["clmAmount"].Value = load.Amount == 0 ? null : load.Amount.ToString("N2", CultureInfo.InvariantCulture);
                row.Cells["clmFullEmpty"].Value = load.FullEmpty is "Full" or "Empty" ? load.FullEmpty : null;
                row.Cells["clmSerialBag"].Value = load.SerialBagNumber;
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
                if (!dataGrid.Rows.Cast<DataGridViewRow>().Any(row => !row.IsNewRow && !IsEmptyRow(row)))
                {
                    MessageBox.Show("Add at least one load before generating the invoice.", "Invoice Builder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var detailsForm = new InvoiceDetailsForm(_invoiceDate, _vesselName))
                {
                    if (detailsForm.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }

                    _invoiceDate = detailsForm.InvoiceDate;
                    _vesselName = detailsForm.VesselName;
                    SaveCurrentDraft();
                }

                var invoice = BuildInvoiceFromUI(validateSourceInvoiceNumbers: true);
                SortLoadsByInvoiceNumber(invoice);

                var fullPath = BuildInvoicePath(invoice);

                var settings = _settingsService.Load();
                LoadingDialog.Run(this, "Generating the invoice and cargo spreadsheet…", () => Task.Run(() =>
                {
                    var cargoSpreadsheetPath = _cargoSpreadsheetService.SaveCargoSpreadsheet(
                        invoice,
                        fullPath,
                        settings.DefaultVehicleRegistration);
                    _pdfInvoiceService.SaveInvoice(invoice, fullPath);
                    _invoiceNumberService.MarkInvoiceNumberUsed(invoice.InvoiceNumber);
                    _generatedInvoiceRepository.Save(BuildGeneratedInvoiceRecord(invoice, fullPath, cargoSpreadsheetPath));
                    _draftService.Clear();
                }));

                MessageBox.Show("Invoice PDF and cargo spreadsheet generated successfully.", "Invoice Builder", MessageBoxButtons.OK, MessageBoxIcon.Information);

                dataGrid.Rows.Clear();
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

        private GeneratedInvoiceRecord BuildGeneratedInvoiceRecord(Invoice invoice, string pdfPath, string cargoSpreadsheetPath)
        {
            var settings = _settingsService.Load();

            return new GeneratedInvoiceRecord
            {
                InvoiceNumber = invoice.InvoiceNumber,
                Date = invoice.Date,
                Customer = invoice.Customer,
                Vessel = invoice.Vessel,
                CargoType = invoice.CargoType,
                VehicleRegistration = settings.DefaultVehicleRegistration,
                Description = string.Empty,
                LoadCount = invoice.Loads.Count,
                StatementDescription = string.Empty,
                Subtotal = invoice.Subtotal,
                Vat = invoice.Vat,
                Total = invoice.Total,
                PdfPath = pdfPath,
                CargoSpreadsheetPath = cargoSpreadsheetPath,
                GeneratedAt = DateTime.Now
            };
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

            var fileName = $"Invoice {invoice.InvoiceNumber} - {monthName} {year} {vessel}.pdf";

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
                && string.IsNullOrWhiteSpace(row.Cells["clmCargoReference"].Value?.ToString())
                && string.IsNullOrWhiteSpace(row.Cells["clmInvoiceNum"].Value?.ToString())
                && row.Cells["clmWeight"].Value is null
                && row.Cells["clmPrice"].Value is null
                && string.IsNullOrWhiteSpace(row.Cells["clmSerialBag"].Value?.ToString())
                && string.IsNullOrWhiteSpace(row.Cells["clmFullEmpty"].Value?.ToString());
        }

        private static void ValidateSourceInvoiceNumbers(Invoice invoice)
        {
            var seenNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var load in invoice.Loads)
            {
                if (string.IsNullOrWhiteSpace(load.SourceInvoiceNumber))
                {
                    throw new InvalidOperationException("Each load row needs its own GRV NR.");
                }

                if (!seenNumbers.Add(load.SourceInvoiceNumber))
                {
                    throw new InvalidOperationException($"GRV NR {load.SourceInvoiceNumber} is duplicated in this invoice.");
                }

                if (string.IsNullOrWhiteSpace(load.CargoReference)
                    && invoice.CargoType != CargoTypes.Spodumene)
                {
                    var fieldName = invoice.CargoType switch
                    {
                        CargoTypes.Containers => "container number",
                        CargoTypes.BreakBulk => "product",
                        _ => "weighbridge ticket number"
                    };
                    throw new InvalidOperationException($"Each load row needs a {fieldName}.");
                }

                if (invoice.CargoType == CargoTypes.Containers
                    && load.FullEmpty is not ("Full" or "Empty"))
                {
                    throw new InvalidOperationException($"Select Full or Empty for GRV NR {load.SourceInvoiceNumber}.");
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
            return cboCalcType.SelectedItem?.ToString()
                ?? (_cboCargoType.SelectedItem?.ToString() == CargoTypes.Containers ? "Flat" : "Weight");
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
