using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceBuilder.Models
{
    public class LoadItem
    {
        public DateTime Date { get; set; }
        public string SourceInvoiceNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string CalcType { get; set; } = "Weight"; // Weight / Flat
        public decimal Amount { get; set; }
    }
}
