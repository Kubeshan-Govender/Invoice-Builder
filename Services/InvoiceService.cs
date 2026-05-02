using InvoiceBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace InvoiceBuilder.Services
{
    public class InvoiceService
    {
        private const decimal VatRate = 0.15m;

        public decimal CalculateAmount(LoadItem item)
        {
            var calcType = item.CalcType?.ToLower();

            return calcType switch
            {
                "weight" => (item.Weight * item.UnitPrice * item.Quantity) / 1000m,
                "flat" => item.UnitPrice * item.Quantity,
                _ => 0
            };
        }

        public void CalculateTotals(Invoice invoice)
        {
            foreach (var item in invoice.Loads)
            {
                item.Amount = CalculateAmount(item);
            }

            invoice.Subtotal = invoice.Loads.Sum(load => load.Amount);
            invoice.Vat = invoice.Subtotal * VatRate * 0;
            invoice.Total = invoice.Subtotal + invoice.Vat;
        }
    }
}
