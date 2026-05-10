namespace InvoiceBuilder.Models
{
    public class AppSettings
    {
        public string AdminPassword { get; set; } = "admin";
        public string DefaultVehicleRegistration { get; set; } = "CR69MZZN";
        public string StatementBillToName { get; set; } = "Everest Holdings";
        public string StatementEmail { get; set; } = "faithtrucking12345@gmail.com";
        public string StatementPhone { get; set; } = "0691157418 / 0828223367";
        public string BankName { get; set; } = string.Empty;
        public string AccountHolder { get; set; } = "FAITH TRUCKING PTY LTD";
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string EmailDefaultRecipient { get; set; } = string.Empty;
        public string EmailSenderName { get; set; } = "Michael Govender";
        public string EmailSenderCompanyName { get; set; } = "Faith Trucking (Pty) Ltd";
    }
}
