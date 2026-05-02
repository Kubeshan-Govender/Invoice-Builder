using InvoiceBuilder.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;

namespace InvoiceBuilder.Services
{
    public class PdfStatementService
    {
        private readonly InvoiceLayoutProfile _layout;

        public PdfStatementService()
            : this(InvoiceLayoutProfiles.Active)
        {
        }

        internal PdfStatementService(InvoiceLayoutProfile layout)
        {
            _layout = layout;
        }

        public void SaveStatement(Statement statement, string filePath)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(_layout.PageMargin);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Content().Column(column =>
                    {
                        ComposeHeader(column, statement);
                        ComposeStatementDetails(column, statement);
                        ComposeLines(column, statement);
                        ComposeBankingDetails(column, statement);
                    });
                });
            }).GeneratePdf(filePath);
        }

        private void ComposeHeader(ColumnDescriptor column, Statement statement)
        {
            column.Item().AlignCenter().Text(_layout.CompanyName).Bold().FontSize(10);
            column.Item().PaddingTop(5).LineHorizontal(1);

            column.Item().PaddingTop(10).AlignCenter().Width(95).Height(55).Element(logo =>
            {
                if (File.Exists(_layout.LogoPath))
                {
                    logo.Image(File.ReadAllBytes(_layout.LogoPath)).FitArea();
                }
                else
                {
                    logo.Border(1).AlignCenter().AlignMiddle().Text("LOGO");
                }
            });

            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Text(text =>
                    {
                        text.Span("Address: ").Bold();
                        text.Span(_layout.CompanyDetails.Count > 0 ? _layout.CompanyDetails[0] : string.Empty);
                    });

                    for (var index = 1; index < _layout.CompanyDetails.Count; index++)
                    {
                        left.Item().PaddingLeft(38).Text(_layout.CompanyDetails[index]);
                    }

                    left.Item().Text(text =>
                    {
                        text.Span("Email: ").Bold();
                        text.Span(_layout.StatementEmail);
                    });
                });

                row.ConstantItem(145).Column(right =>
                {
                    right.Item().Text(text =>
                    {
                        text.Span("Phone: ").Bold();
                        text.Span(_layout.StatementPhone);
                    });
                });
            });

            column.Item().PaddingTop(10).LineHorizontal(1);
            column.Item().PaddingVertical(6).AlignCenter().Text("Statement").FontSize(12);
            column.Item().LineHorizontal(1);
        }

        private void ComposeStatementDetails(ColumnDescriptor column, Statement statement)
        {
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Text($"Statement #: {statement.StatementNumber}").Bold();
                    left.Item().Text($"Date: {statement.Date:MMMM d, yyyy}").Bold();
                    left.Item().Text($"Customer ID: {statement.CustomerId}").Bold();
                });

                row.ConstantItem(170).Column(right =>
                {
                    right.Item().Text(text =>
                    {
                        text.Span("Bill To: ").Bold();
                        text.Span(statement.BillToName);
                    });

                    foreach (var line in _layout.StatementBillToDetails)
                    {
                        right.Item().PaddingLeft(31).Text(line);
                    }
                });
            });
        }

        private void ComposeLines(ColumnDescriptor column, Statement statement)
        {
            column.Item().PaddingTop(12).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.ConstantColumn(18);
                    columns.ConstantColumn(90);
                });

                RemittanceCell(table.Cell(), "Remittance Amount Enclosed:");
                RemittanceCell(table.Cell(), "R", TextAlignment.Center);
                RemittanceCell(table.Cell(), $"{statement.Total:N2}", TextAlignment.Right);
            });

            column.Item().PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(70);
                    columns.RelativeColumn();
                });

                HeaderCell(table.Cell(), "Vessel");
                BodyCell(table.Cell(), statement.Vessel, TextAlignment.Center, bold: true);
            });

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(65);
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(65);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(60);
                    columns.ConstantColumn(70);
                });

                table.Header(header =>
                {
                    HeaderCell(header.Cell(), "Date");
                    HeaderCell(header.Cell(), "Description");
                    HeaderCell(header.Cell(), "Invoice #");
                    HeaderCell(header.Cell(), "Vehicle Reg\nNumber");
                    HeaderCell(header.Cell(), "Amount");
                    HeaderCell(header.Cell(), "Payment");
                    HeaderCell(header.Cell(), "Total");
                });

                foreach (var line in statement.Lines)
                {
                    BodyCell(table.Cell(), line.Date.ToString("yyyy/MM/dd"), TextAlignment.Center);
                    BodyCell(table.Cell(), line.Description);
                    BodyCell(table.Cell(), line.InvoiceNumber, TextAlignment.Center);
                    BodyCell(table.Cell(), line.VehicleRegistration, TextAlignment.Center);
                    MoneyCell(table.Cell(), line.Amount);
                    MoneyCell(table.Cell(), line.Payment);
                    MoneyCell(table.Cell(), line.Total);
                }

                var blankRows = Math.Max(4, 8 - statement.Lines.Count);
                for (var row = 0; row < blankRows; row++)
                {
                    BodyCell(table.Cell(), string.Empty);
                    BodyCell(table.Cell(), string.Empty);
                    BodyCell(table.Cell(), string.Empty);
                    BodyCell(table.Cell(), string.Empty);
                    BodyCell(table.Cell(), string.Empty);
                    BodyCell(table.Cell(), string.Empty);
                    BodyCell(table.Cell(), string.Empty);
                }

                BodyCell(table.Cell().ColumnSpan(6), string.Empty);
                MoneyCell(table.Cell(), statement.Total, bold: true);
            });

            column.Item().PaddingTop(12).Text("Reminder: Please include the statement number on your check.").Bold();
            column.Item().PaddingTop(8).Text(statement.StatementNumber.ToString()).Bold();
            column.Item().PaddingTop(5).LineHorizontal(1);
        }

        private void ComposeBankingDetails(ColumnDescriptor column, Statement statement)
        {
            column.Item().PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(210);
                    columns.RelativeColumn();
                });

                HeaderCell(table.Cell().ColumnSpan(2), "Banking Details", TextAlignment.Left);
                BankingCell(table.Cell(), "Name of Bank :");
                BankingCell(table.Cell(), _layout.BankName);
                BankingCell(table.Cell(), "Account Holder:");
                BankingCell(table.Cell(), _layout.AccountHolder);
                BankingCell(table.Cell(), "Account Number:");
                BankingCell(table.Cell(), _layout.AccountNumber);
                BankingCell(table.Cell(), "Type of Account:");
                BankingCell(table.Cell(), _layout.AccountType);
                BankingCell(table.Cell(), "Branch Code:");
                BankingCell(table.Cell(), _layout.BranchCode);
                BankingCell(table.Cell(), "Total Amount:");
                BankingCell(table.Cell(), $"R {statement.Total:N2}");
            });
        }

        private void HeaderCell(IContainer cell, string text, TextAlignment alignment = TextAlignment.Center)
        {
            var content = cell.Border(1)
                .BorderColor(Colors.Black)
                .Background(Colors.Grey.Darken2)
                .Padding(5)
                .AlignMiddle();

            content = Align(content, alignment);
            content.Text(text).FontColor(Colors.White).Bold();
        }

        private void BodyCell(IContainer cell, string text, TextAlignment alignment = TextAlignment.Left, bool bold = false)
        {
            var content = cell.Border(1)
                .BorderColor(Colors.Black)
                .Padding(5)
                .AlignMiddle();

            content = Align(content, alignment);

            var textDescriptor = content.Text(text);
            if (bold)
            {
                textDescriptor.Bold();
            }
        }

        private void RemittanceCell(IContainer cell, string text, TextAlignment alignment = TextAlignment.Left)
        {
            var content = cell.Background(Colors.Grey.Darken2)
                .Padding(4)
                .AlignMiddle();

            content = Align(content, alignment);
            content.Text(text).FontColor(Colors.White).Bold();
        }

        private void BankingCell(IContainer cell, string text)
        {
            cell.Border(1)
                .BorderColor(Colors.Black)
                .Padding(5)
                .Text(text)
                .Bold();
        }

        private void MoneyCell(IContainer cell, decimal amount, bool bold = false)
        {
            BodyCell(cell, amount == 0 ? string.Empty : $"R {amount:N2}", TextAlignment.Right, bold);
        }

        private static IContainer Align(IContainer container, TextAlignment alignment)
        {
            return alignment switch
            {
                TextAlignment.Center => container.AlignCenter(),
                TextAlignment.Right => container.AlignRight(),
                _ => container
            };
        }

        private enum TextAlignment
        {
            Left,
            Center,
            Right
        }
    }
}
