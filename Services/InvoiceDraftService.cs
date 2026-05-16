using InvoiceBuilder.Models;
using System;
using System.IO;
using System.Text.Json;

namespace InvoiceBuilder.Services
{
    public class InvoiceDraftService
    {
        private readonly string _filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "InvoiceBuilder",
            "current_invoice_draft.json");

        public Invoice Load()
        {
            if (!File.Exists(_filePath))
            {
                return null;
            }

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<Invoice>(json);
        }

        public void Save(Invoice invoice)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(invoice, new JsonSerializerOptions { WriteIndented = true }));
        }

        public void Clear()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
    }
}
