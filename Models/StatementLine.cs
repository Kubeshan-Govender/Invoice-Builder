using System;

namespace InvoiceBuilder.Models
{
    public class StatementLine
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string VehicleRegistration { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Payment { get; set; }
        public decimal Total => Amount - Payment;
    }
}
