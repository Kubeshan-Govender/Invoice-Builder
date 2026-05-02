using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace InvoiceBuilder.Services
{
    public class InvoiceNumberService
    {
        private readonly string _filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "InvoiceBuilder",
            "invoice_number.txt");

        public int GetNextInvoiceNumberPreview()
        {
            EnsureFileExists();
            return int.Parse(File.ReadAllText(_filePath)) + 1;
        }

        public void MarkInvoiceNumberUsed(int invoiceNumber)
        {
            EnsureFileExists();
            var currentNumber = int.Parse(File.ReadAllText(_filePath));
            if (invoiceNumber > currentNumber)
            {
                File.WriteAllText(_filePath, invoiceNumber.ToString());
            }
        }

        private void EnsureFileExists()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "0");
        }
    }
}
