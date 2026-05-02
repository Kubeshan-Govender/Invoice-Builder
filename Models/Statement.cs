using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceBuilder.Models
{
    public class Statement
    {
        public int StatementNumber { get; set; }
        public DateTime Date { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string BillToName { get; set; } = string.Empty;
        public string Vessel { get; set; } = string.Empty;
        public List<StatementLine> Lines { get; set; } = new();
        public decimal Total => Lines.Sum(line => line.Total);
    }
}
