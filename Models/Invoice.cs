using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceBuilder.Models
{
    public class Invoice
    {
        public int InvoiceNumber { get; set; }
        public DateTime Date { get; set; }
        public string Vessel { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public List<LoadItem> Loads { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Vat { get; set; }
        public decimal Total { get; set; }
    }
}
