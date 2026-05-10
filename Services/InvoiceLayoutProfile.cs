using InvoiceBuilder.Models;
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;

namespace InvoiceBuilder.Services
{
    internal enum InvoiceTextAlignment
    {
        Left,
        Center,
        Right
    }

    internal sealed class InvoiceTableColumn
    {
        public string Header { get; init; } = string.Empty;
        public float Width { get; init; } = 1;
        public bool UseRelativeWidth { get; init; }
        public InvoiceTextAlignment Alignment { get; init; } = InvoiceTextAlignment.Left;
        public Func<LoadItem, int, string> Value { get; init; } = (_, _) => string.Empty;
    }

    internal sealed class InvoiceLayoutProfile
    {
        public float PageMargin { get; init; } = 30;
        public int DefaultFontSize { get; init; } = 10;
        public string ThemeColor { get; init; } = Colors.Blue.Darken4;
        public string AccentColor { get; init; } = Colors.Blue.Medium;
        public string BorderColor { get; init; } = Colors.Grey.Lighten1;

        public string CompanyName { get; init; } = string.Empty;
        public string FooterCompanyName { get; init; } = string.Empty;
        public string LogoPath { get; init; } = string.Empty;
        public int LogoWidth { get; init; } = 120;
        public int LogoHeight { get; init; } = 70;
        public IReadOnlyList<string> CompanyDetails { get; init; } = Array.Empty<string>();

        public string InvoiceTitle { get; init; } = "INVOICE";
        public int InvoiceTitleFontSize { get; init; } = 28;
        public int InvoiceMetaWidth { get; init; } = 220;
        public string Terms { get; init; } = string.Empty;

        public string BillToTitle { get; init; } = "BILL TO";
        public IReadOnlyList<string> BillToDetails { get; init; } = Array.Empty<string>();
        public string VesselLabel { get; init; } = "VESSEL";

        public string ContactNote { get; init; } = string.Empty;
        public string ContactEmail { get; init; } = string.Empty;
        public IReadOnlyList<InvoiceTableColumn> LoadTableColumns { get; init; } = Array.Empty<InvoiceTableColumn>();

        public string StatementEmail { get; init; } = string.Empty;
        public string StatementPhone { get; init; } = string.Empty;
        public string StatementBillToName { get; init; } = string.Empty;
        public IReadOnlyList<string> StatementBillToDetails { get; init; } = Array.Empty<string>();
        public string StatementTripUnitSingular { get; init; } = "LOAD";
        public string StatementTripUnitPlural { get; init; } = "LOADS";
        public string BankName { get; init; } = string.Empty;
        public string AccountHolder { get; init; } = string.Empty;
        public string AccountNumber { get; init; } = string.Empty;
        public string AccountType { get; init; } = string.Empty;
        public string BranchCode { get; init; } = string.Empty;

        public string EmailDefaultRecipient { get; init; } = string.Empty;
        public string EmailSenderName { get; init; } = string.Empty;
        public string EmailSenderCompanyName { get; init; } = string.Empty;
    }

    internal static class InvoiceLayoutProfiles
    {
        private const string ActiveProfile = "FaithTrucking";

        public static InvoiceLayoutProfile Active => ForCompany(ActiveProfile);

        // Core layout stays in code; selected business defaults can be edited from Admin Settings.
        public static InvoiceLayoutProfile FaithTrucking => BuildFaithTrucking(new AppSettings());

        private static InvoiceLayoutProfile BuildFaithTrucking(AppSettings settings)
        {
            return new InvoiceLayoutProfile
            {
                CompanyName = "FAITH TRUCKING (PTY) LTD",
                FooterCompanyName = "FAITH TRUCKING PTY(LTD)",
                LogoPath = "Media/logo.jpeg",
                CompanyDetails =
                [
                    "55 OCEANRIDGE DRIVE",
                    "VERULAM",
                    "Phone: 0691157481 / 0828223367"
                ],
                Terms = "15 Days",
                BillToDetails =
                [
                    "OP Shed",
                    "T-Jetty",
                    "Durban Harbour"
                ],
                ContactNote = "For more information and questions about the above invoice please contact:",
                ContactEmail = "michaelgovender12345@gmail.com",
                StatementEmail = settings.StatementEmail,
                StatementPhone = settings.StatementPhone,
                StatementBillToName = settings.StatementBillToName,
                StatementBillToDetails =
                [
                    "OP Shed",
                    "T-JETTY",
                    "Durban Harbour"
                ],
                BankName = settings.BankName,
                AccountHolder = settings.AccountHolder,
                AccountNumber = settings.AccountNumber,
                AccountType = settings.AccountType,
                BranchCode = settings.BranchCode,
                EmailDefaultRecipient = settings.EmailDefaultRecipient,
                EmailSenderName = settings.EmailSenderName,
                EmailSenderCompanyName = settings.EmailSenderCompanyName,
                LoadTableColumns =
                [
                    new()
                    {
                        Header = "No.",
                        Width = 30,
                        Alignment = InvoiceTextAlignment.Center,
                        Value = (_, index) => index.ToString()
                    },
                    new()
                    {
                        Header = "Date",
                        Width = 70,
                        Alignment = InvoiceTextAlignment.Center,
                        Value = (item, _) => item.Date.ToString("yyyy/MM/dd")
                    },
                    new()
                    {
                        Header = "Description",
                        Width = 3,
                        UseRelativeWidth = true,
                        Value = (item, _) => item.Description
                    },
                    new()
                    {
                        Header = "Weight",
                        Width = 70,
                        Alignment = InvoiceTextAlignment.Right,
                        Value = (item, _) => item.Weight.ToString("N0")
                    },
                    new()
                    {
                        Header = "Qty",
                        Width = 40,
                        Alignment = InvoiceTextAlignment.Center,
                        Value = (item, _) => item.Quantity.ToString()
                    },
                    new()
                    {
                        Header = "Unit Price",
                        Width = 80,
                        Alignment = InvoiceTextAlignment.Right,
                        Value = (item, _) => $"R {item.UnitPrice:N2}"
                    },
                    new()
                    {
                        Header = "Amount",
                        Width = 80,
                        Alignment = InvoiceTextAlignment.Right,
                        Value = (item, _) => $"R {item.Amount:N2}"
                    }
                ]
            };
        }

        public static InvoiceLayoutProfile ForCompany(string companyKey)
        {
            return companyKey switch
            {
                "FaithTrucking" => BuildFaithTrucking(new AppSettingsService().Load()),
                _ => BuildFaithTrucking(new AppSettingsService().Load())
            };
        }
    }
}
