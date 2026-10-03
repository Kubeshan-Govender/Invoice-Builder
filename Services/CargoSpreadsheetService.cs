using ClosedXML.Excel;
using InvoiceBuilder.Models;
using System;
using System.IO;

namespace InvoiceBuilder.Services
{
    public class CargoSpreadsheetService
    {
        public string SaveCargoSpreadsheet(Invoice invoice, string invoicePdfPath, string vehicleRegistration)
        {
            var templatePath = GetTemplatePath(invoice.CargoType);
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"The {invoice.CargoType} cargo spreadsheet template is missing.", templatePath);
            }

            var year = invoice.Date.Year.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var monthYear = invoice.Date.ToString("MMMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var outputFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "Faith Trucking",
                "Books",
                "Everest Docs",
                year,
                monthYear);
            Directory.CreateDirectory(outputFolder);
            var outputPath = Path.Combine(outputFolder, $"{Path.GetFileNameWithoutExtension(invoicePdfPath)} - {invoice.CargoType}.xlsx");

            File.Copy(templatePath, outputPath, overwrite: true);

            using var workbook = new XLWorkbook(outputPath);
            var sheet = workbook.Worksheet(1);
            var transporter = "Faith Trucking";
            const int firstDataRow = 2;
            var capacity = invoice.CargoType switch
            {
                CargoTypes.Spodumene => 1557,
                CargoTypes.Containers => 95,
                CargoTypes.BreakBulk => 186,
                _ => throw new InvalidOperationException($"Unsupported cargo type '{invoice.CargoType}'.")
            };

            if (invoice.Loads.Count > capacity)
            {
                throw new InvalidOperationException($"The {invoice.CargoType} spreadsheet supports up to {capacity} loads per invoice.");
            }

            if (invoice.CargoType == CargoTypes.Spodumene)
            {
                sheet.Column(6).InsertColumnsBefore(1);
                sheet.Column(6).Width = sheet.Column(5).Width;
                for (var row = 1; row <= 1559; row++)
                {
                    sheet.Cell(row, 6).Style = sheet.Cell(row, 5).Style;
                }
                sheet.Cell("F1").Value = "DESCRIPTION";
                sheet.Cell("D1").Value = "GRV NR";
                sheet.Cell("G1559").FormulaA1 = "SUM(G2:G1558)";
            }

            for (var index = 0; index < invoice.Loads.Count; index++)
            {
                var row = firstDataRow + index;
                var load = invoice.Loads[index];

                sheet.Cell(row, 1).Value = load.Date;
                sheet.Cell(row, 2).Value = vehicleRegistration;
                sheet.Cell(row, 3).Value = transporter;
                sheet.Cell(row, 4).Value = load.SourceInvoiceNumber;

                switch (invoice.CargoType)
                {
                    case CargoTypes.Spodumene:
                        // This template has no RATE field. The entered rate is used on the invoice PDF.
                        sheet.Cell(row, 5).Value = load.CargoReference; // Weighbridge ticket number; optional.
                        sheet.Cell(row, 6).Value = load.Description;
                        sheet.Cell(row, 7).Value = load.Weight;
                        break;

                    case CargoTypes.Containers:
                        sheet.Cell(row, 5).Value = load.CargoReference; // Container number
                        sheet.Cell(row, 6).Value = string.Empty; // ISO group is optional and currently left blank.
                        sheet.Cell(row, 7).Value = load.FullEmpty;
                        sheet.Cell(row, 8).Value = load.Weight;
                        sheet.Cell(row, 9).Value = load.UnitPrice;
                        break;

                    case CargoTypes.BreakBulk:
                        sheet.Cell(row, 5).Value = load.CargoReference; // Product
                        sheet.Cell(row, 6).Value = load.SerialBagNumber;
                        sheet.Cell(row, 7).Value = load.Quantity;
                        sheet.Cell(row, 8).Value = load.Weight;
                        sheet.Cell(row, 9).Value = load.UnitPrice;
                        break;
                }
            }

            workbook.Save();
            return outputPath;
        }

        private static string GetTemplatePath(string cargoType)
        {
            var fileName = cargoType switch
            {
                CargoTypes.Spodumene => "Spodumene.xlsx",
                CargoTypes.Containers => "Containers.xlsx",
                CargoTypes.BreakBulk => "Break Bulk.xlsx",
                _ => throw new InvalidOperationException($"Unsupported cargo type '{cargoType}'.")
            };

            return Path.Combine(AppContext.BaseDirectory, "Templates", fileName);
        }
    }
}
