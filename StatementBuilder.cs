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
    public class StatementBuilder : Form
    {
        private readonly GeneratedInvoiceRepository _invoiceRepository = new();
        private readonly StatementNumberService _statementNumberService = new();
        private readonly PdfStatementService _pdfStatementService = new();
        private readonly EmailDraftService _emailDraftService = new();

        private readonly DataGridView _invoiceGrid = new();
        private readonly Label _statementNumber = new();
        private readonly DateTimePicker _statementDate = new();
        private readonly TextBox _customerId = new();
        private readonly TextBox _billTo = new();
        private readonly TextBox _vessel = new();
        private readonly Label _total = new();
        private readonly Button _generate = new();
        private readonly Button _refresh = new();

        private List<GeneratedInvoiceRecord> _invoices = new();

        public StatementBuilder()
        {
            Text = "Statement Builder";
            Width = 1050;
            Height = 650;
            StartPosition = FormStartPosition.CenterParent;

            BuildLayout();
            ApplyVisualStyle();
            LoadInvoices();
        }

        private void BuildLayout()
        {
            var main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(10)
            };

            main.RowStyles.Add(new RowStyle());
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            main.RowStyles.Add(new RowStyle());
            main.RowStyles.Add(new RowStyle());

            var header = BuildHeader();
            ConfigureInvoiceGrid();

            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true
            };

            _generate.Text = "Generate Statement";
            _generate.Width = 180;
            _generate.Height = 32;
            _generate.Click += (_, _) => GenerateStatement();

            _refresh.Text = "Refresh Invoices";
            _refresh.Width = 140;
            _refresh.Height = 32;
            _refresh.Click += (_, _) => LoadInvoices();

            _total.AutoSize = true;
            _total.TextAlign = ContentAlignment.MiddleRight;
            _total.Padding = new Padding(0, 8, 20, 0);

            footer.Controls.Add(_generate);
            footer.Controls.Add(_refresh);
            footer.Controls.Add(_total);

            main.Controls.Add(header, 0, 0);
            main.Controls.Add(_invoiceGrid, 0, 1);
            main.Controls.Add(new Label { Text = "Tick one or more printed invoices. Type the statement description manually for each selected invoice before generating.", AutoSize = true }, 0, 2);
            main.Controls.Add(footer, 0, 3);

            Controls.Add(main);
        }

        private void ApplyVisualStyle()
        {
            AppStyles.ApplyForm(this);
            Text = "Faith Trucking Statement Builder";
            MinimumSize = new Size(980, 640);

            foreach (var table in Controls.OfType<TableLayoutPanel>())
            {
                table.BackColor = AppStyles.Surface;
            }

            AppStyles.ApplyButton(_generate, ButtonRole.Primary);
            AppStyles.ApplyButton(_refresh, ButtonRole.Secondary);
            AppStyles.ApplyTotalLabel(_total, emphasized: true);
            _vessel.BackColor = Color.FromArgb(252, 248, 238);
            _vessel.BorderStyle = BorderStyle.FixedSingle;
        }

        private TableLayoutPanel BuildHeader()
        {
            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4,
                AutoSize = true
            };

            for (var i = 0; i < 4; i++)
            {
                header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            }

            AddLabel(header, "Statement #", 0, 0);
            AddLabel(header, "Statement Date", 1, 0);
            AddLabel(header, "Customer ID", 2, 0);
            AddLabel(header, "Bill To", 3, 0);

            _statementNumber.Dock = DockStyle.Fill;
            _statementDate.Dock = DockStyle.Fill;
            _customerId.Dock = DockStyle.Fill;
            _billTo.Dock = DockStyle.Fill;

            header.Controls.Add(_statementNumber, 0, 1);
            header.Controls.Add(_statementDate, 1, 1);
            header.Controls.Add(_customerId, 2, 1);
            header.Controls.Add(_billTo, 3, 1);

            AddLabel(header, "Vessels", 0, 2);
            _vessel.Dock = DockStyle.Fill;
            header.Controls.Add(_vessel, 0, 3);
            header.SetColumnSpan(_vessel, 4);

            return header;
        }

        private void ConfigureInvoiceGrid()
        {
            _invoiceGrid.Dock = DockStyle.Fill;
            _invoiceGrid.AllowUserToAddRows = false;
            _invoiceGrid.AllowUserToDeleteRows = false;
            _invoiceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _invoiceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _invoiceGrid.MultiSelect = false;

            _invoiceGrid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "clmInclude", HeaderText = "", FillWeight = 35 });
            _invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "clmDate", HeaderText = "Date", ReadOnly = true, FillWeight = 75 });
            _invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "clmInvoice", HeaderText = "Invoice #", ReadOnly = true, FillWeight = 75 });
            _invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "clmVessel", HeaderText = "Vessel", ReadOnly = true, FillWeight = 120 });
            _invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "clmLoads", HeaderText = "Loads/Trips", ReadOnly = true, FillWeight = 75 });
            _invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "clmDescription", HeaderText = "Statement Description", FillWeight = 260 });
            _invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "clmAmount", HeaderText = "Amount", ReadOnly = true, FillWeight = 90 });

            _invoiceGrid.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (_invoiceGrid.IsCurrentCellDirty)
                {
                    _invoiceGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };

            _invoiceGrid.CellValueChanged += (_, _) => UpdateTotalsFromSelection();
        }

        private static void AddLabel(TableLayoutPanel panel, string text, int column, int row)
        {
            panel.Controls.Add(new Label { Text = text, Dock = DockStyle.Fill, AutoSize = true }, column, row);
        }

        private void LoadInvoices()
        {
            _invoices = _invoiceRepository.GetAll();
            _invoiceGrid.Rows.Clear();

            foreach (var invoice in _invoices)
            {
                var rowIndex = _invoiceGrid.Rows.Add(
                    false,
                    invoice.Date.ToString("yyyy/MM/dd"),
                    invoice.InvoiceNumber.ToString(CultureInfo.InvariantCulture),
                    invoice.Vessel,
                    invoice.LoadCount == 0 ? string.Empty : invoice.LoadCount.ToString(CultureInfo.InvariantCulture),
                    string.Empty,
                    $"R {invoice.Total:N2}");

                _invoiceGrid.Rows[rowIndex].Tag = invoice;
            }

            _statementNumber.Text = _statementNumberService.GetNextStatementNumberPreview().ToString(CultureInfo.InvariantCulture);
            _statementDate.Value = DateTime.Today;

            if (_invoices.Count > 0)
            {
                _customerId.Text = _invoices[0].Customer;
                _billTo.Text = InvoiceLayoutProfiles.Active.StatementBillToName;
            }

            UpdateTotalsFromSelection();
        }

        private void UpdateTotalsFromSelection()
        {
            var selected = GetSelectedRows();
            _total.Text = $"Selected Total: R {selected.Sum(row => row.Invoice.Total):N2}";
            _vessel.Text = BuildCombinedVesselName(selected.Select(row => row.Invoice));

            if (selected.Count == 1)
            {
                _customerId.Text = selected[0].Invoice.Customer;
            }
        }

        private List<SelectedStatementRow> GetSelectedRows()
        {
            var selected = new List<SelectedStatementRow>();

            foreach (DataGridViewRow row in _invoiceGrid.Rows)
            {
                var include = row.Cells["clmInclude"].Value is bool value && value;
                if (!include || row.Tag is not GeneratedInvoiceRecord invoice)
                {
                    continue;
                }

                selected.Add(new SelectedStatementRow(
                    invoice,
                    row.Cells["clmDescription"].Value?.ToString()?.Trim() ?? string.Empty));
            }

            return selected
                .OrderBy(row => row.Invoice.Date)
                .ThenBy(row => row.Invoice.InvoiceNumber)
                .ToList();
        }

        private static string BuildCombinedVesselName(IEnumerable<GeneratedInvoiceRecord> invoices)
        {
            return string.Join(", ", invoices
                .Select(invoice => invoice.Vessel.Trim())
                .Where(vessel => !string.IsNullOrWhiteSpace(vessel))
                .Distinct(StringComparer.OrdinalIgnoreCase));
        }

        private void GenerateStatement()
        {
            try
            {
                var selectedRows = GetSelectedRows();
                if (selectedRows.Count == 0)
                {
                    MessageBox.Show("Select at least one invoice to include in the statement.", "Statement Builder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var statement = BuildStatement(selectedRows);
                var filePath = BuildStatementPath(statement);

                _pdfStatementService.SaveStatement(statement, filePath);
                _statementNumberService.MarkStatementNumberUsed(statement.StatementNumber);

                var emailResult = _emailDraftService.CreateStatementEmailDraft(
                    statement,
                    filePath,
                    selectedRows.Select(row => row.Invoice.PdfPath));

                MessageBox.Show($"Statement generated successfully.\n\n{filePath}\n\n{emailResult}", "Statement Builder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadInvoices();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not generate the statement. {ex.Message}", "Statement Builder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Statement BuildStatement(List<SelectedStatementRow> selectedRows)
        {
            return new Statement
            {
                StatementNumber = int.Parse(_statementNumber.Text, CultureInfo.InvariantCulture),
                Date = _statementDate.Value,
                CustomerId = _customerId.Text.Trim(),
                BillToName = _billTo.Text.Trim(),
                Vessel = _vessel.Text.Trim(),
                Lines = selectedRows.Select(row => new StatementLine
                {
                    Date = row.Invoice.Date,
                    Description = BuildLineDescription(row.Invoice, row.Description),
                    InvoiceNumber = row.Invoice.InvoiceNumber.ToString(CultureInfo.InvariantCulture),
                    VehicleRegistration = "CR69MZZN",
                    Amount = row.Invoice.Total
                }).ToList()
            };
        }

        private static string BuildLineDescription(GeneratedInvoiceRecord invoice, string userDescription)
        {
            if (invoice.LoadCount <= 0)
            {
                return string.IsNullOrWhiteSpace(userDescription) ? invoice.Description : userDescription;
            }

            var unit = invoice.LoadCount == 1
                ? InvoiceLayoutProfiles.Active.StatementTripUnitSingular
                : InvoiceLayoutProfiles.Active.StatementTripUnitPlural;

            var prefix = $"{invoice.LoadCount} X {unit}";
            return string.IsNullOrWhiteSpace(userDescription) ? prefix : $"{prefix} {userDescription}";
        }

        private static string BuildStatementPath(Statement statement)
        {
            var year = statement.Date.Year;
            var monthName = statement.Date.ToString("MMMM", CultureInfo.InvariantCulture);
            var monthYear = $"{monthName} {year}";

            var baseFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "Faith Trucking",
                "Books",
                year.ToString(CultureInfo.InvariantCulture),
                monthYear,
                "Statements");

            Directory.CreateDirectory(baseFolder);

            var vessel = SanitizeFileName(statement.Vessel).Replace(", ", " ");
            var fileName = string.IsNullOrWhiteSpace(vessel)
                ? $"Statement {monthName} {year} #{statement.StatementNumber}.pdf"
                : $"Statement {monthName} {year} {vessel}.pdf";

            return Path.Combine(baseFolder, fileName);
        }

        private static string SanitizeFileName(string input)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c.ToString(), string.Empty);
            }

            return input.Trim();
        }

        private sealed class SelectedStatementRow
        {
            public SelectedStatementRow(GeneratedInvoiceRecord invoice, string description)
            {
                Invoice = invoice;
                Description = description;
            }

            public GeneratedInvoiceRecord Invoice { get; }
            public string Description { get; }
        }
    }
}
