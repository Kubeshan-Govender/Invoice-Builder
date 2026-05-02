using InvoiceBuilder.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;

namespace InvoiceBuilder.Services
{
    public class PdfInvoiceService
    {
        private readonly InvoiceLayoutProfile _layout;

        public PdfInvoiceService()
            : this(InvoiceLayoutProfiles.Active)
        {
        }

        internal PdfInvoiceService(InvoiceLayoutProfile layout)
        {
            _layout = layout;
        }

        public void SaveInvoice(Invoice invoice, string filePath)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(_layout.PageMargin);

                    page.DefaultTextStyle(x => x.FontSize(_layout.DefaultFontSize));

                    page.Header().Element(header => ComposeHeader(header, invoice));

                    page.Content().Element(content => ComposeContent(content, invoice));

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().AlignLeft().Text(_layout.FooterCompanyName).Bold();
                        row.RelativeItem().AlignRight().Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                    });
                });
            }).GeneratePdf(filePath);
        }

        // ---------------- HEADER ----------------
        private void ComposeHeader(IContainer container, Invoice invoice)
        {
            container.Column(column =>
            {
                // First row: Logo + Invoice meta
                column.Item().Row(row =>
                {
                    // LEFT: Logo + Company Info
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(_layout.CompanyName)
                            .Bold().FontSize(16).FontColor(_layout.ThemeColor);

                        var logo = col.Item().Height(_layout.LogoHeight).Width(_layout.LogoWidth)
                            .Border(1)
                            .AlignCenter()
                            .AlignMiddle();

                        if (File.Exists(_layout.LogoPath))
                        {
                            logo.Image(File.ReadAllBytes(_layout.LogoPath)).FitArea();
                        }
                        else
                        {
                            logo.Text("LOGO").FontColor(Colors.Grey.Darken1);
                        }

                        foreach (var line in _layout.CompanyDetails)
                        {
                            col.Item().Text(line).Bold();
                        }
                    });

                    // RIGHT: Invoice Title + Meta
                    row.ConstantItem(_layout.InvoiceMetaWidth).Column(col =>
                    {
                        col.Item().AlignRight().Text(_layout.InvoiceTitle)
                            .FontSize(_layout.InvoiceTitleFontSize)
                            .Bold()
                            .FontColor(_layout.ThemeColor);

                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            table.Cell().Background(_layout.ThemeColor).Padding(5).Text("INVOICE #").FontColor(Colors.White);
                            table.Cell().Background(_layout.ThemeColor).Padding(5).Text("DATE").FontColor(Colors.White);

                            table.Cell().Padding(5).Text(invoice.InvoiceNumber.ToString());
                            table.Cell().Padding(5).Text(invoice.Date.ToString("yyyy/MM/dd"));

                            table.Cell().Background(_layout.ThemeColor).Padding(5).Text("CUSTOMER").FontColor(Colors.White);
                            table.Cell().Background(_layout.ThemeColor).Padding(5).Text("TERMS").FontColor(Colors.White);

                            table.Cell().Padding(5).Text(invoice.Customer);
                            table.Cell().Padding(5).Text(_layout.Terms);
                        });
                    });
                });

                // Second row: Bill To
                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Background(_layout.ThemeColor)
                            .Padding(5)
                            .Text(_layout.BillToTitle)
                            .FontColor(Colors.White);

                        col.Item().Padding(5).Text(invoice.Customer);
                        foreach (var line in _layout.BillToDetails)
                        {
                            col.Item().PaddingLeft(5).Text(line);
                        }

                        col.Item().PaddingTop(10).Text($"{_layout.VesselLabel}: {invoice.Vessel}").Bold();
                    });
                });
            });
        }

        // ---------------- CONTENT ----------------
        private void ComposeContent(IContainer container, Invoice invoice)
        {
            container.Column(col =>
            {
                col.Item().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        foreach (var column in _layout.LoadTableColumns)
                        {
                            if (column.UseRelativeWidth)
                            {
                                columns.RelativeColumn(column.Width);
                            }
                            else
                            {
                                columns.ConstantColumn(column.Width);
                            }
                        }
                    });

                    table.Header(header =>
                    {
                        void HeaderCell(IContainer cell, string text)
                        {
                            cell.Border(1)
                                .BorderColor(Colors.White)
                                .Background(_layout.ThemeColor)
                                .Padding(5)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text(text)
                                .FontColor(Colors.White)
                                .Bold();
                        }

                        foreach (var column in _layout.LoadTableColumns)
                        {
                            header.Cell().Element(c => HeaderCell(c, column.Header));
                        }
                    });

                    int index = 1;
                    foreach (var item in invoice.Loads)
                    {
                        foreach (var column in _layout.LoadTableColumns)
                        {
                            table.Cell().Element(cell => LoadCell(cell, column, item, index));
                        }

                        index++;
                    }
                });

                col.Item().PaddingTop(20).AlignRight().Column(totals =>
                {
                    totals.Item().Row(r =>
                    {
                        r.RelativeItem().Text("SUBTOTAL").Bold();
                        r.ConstantItem(100).AlignRight().Text($"R {invoice.Subtotal:N2}");
                    });

                    totals.Item().Row(r =>
                    {
                        r.RelativeItem().Text("VAT").Bold();
                        r.ConstantItem(100).AlignRight().Text(invoice.Vat == 0 ? "NON VAT" : $"R {invoice.Vat:N2}");
                    });

                    totals.Item().LineHorizontal(1);

                    totals.Item().Row(r =>
                    {
                        r.RelativeItem().Text("TOTAL").Bold().FontSize(12);
                        r.ConstantItem(100).AlignRight().Text($"R {invoice.Total:N2}").Bold().FontSize(12);
                    });
                });
                col.Item().PaddingTop(20).Column(note =>
                {
                    note.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    note.Item().PaddingTop(10).Text(
                        _layout.ContactNote
                    ).FontSize(10);

                    note.Item().Text(_layout.ContactEmail)
                        .FontSize(10)
                        .Bold()
                        .FontColor(_layout.AccentColor);
                });
            });
        }

        private void LoadCell(IContainer cell, InvoiceTableColumn column, LoadItem item, int rowNumber)
        {
            var content = cell.Border(1)
                .BorderColor(_layout.BorderColor)
                .Padding(5)
                .AlignMiddle();

            content = column.Alignment switch
            {
                InvoiceTextAlignment.Center => content.AlignCenter(),
                InvoiceTextAlignment.Right => content.AlignRight(),
                _ => content
            };

            content.Text(column.Value(item, rowNumber));
        }
    }
}
