using System;

namespace InvoiceBuilder.Models
{
    public class GeneratedInvoiceRecord
    {
        public int InvoiceNumber { get; set; }
        public DateTime Date { get; set; }
        public string Customer { get; set; } = string.Empty;
        public string Vessel { get; set; } = string.Empty;
        public string VehicleRegistration { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int LoadCount { get; set; }
        public string StatementDescription { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Vat { get; set; }
        public decimal Total { get; set; }
        public string PdfPath { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }
}
