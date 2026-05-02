using InvoiceBuilder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace InvoiceBuilder.Repositories
{
    public class GeneratedInvoiceRepository
    {
        private readonly string _filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "InvoiceBuilder",
            "generated_invoices.json");

        public List<GeneratedInvoiceRecord> GetAll()
        {
            EnsureStoreExists();
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<GeneratedInvoiceRecord>>(json) ?? new List<GeneratedInvoiceRecord>();
        }

        public void Save(GeneratedInvoiceRecord record)
        {
            var records = GetAll();
            records.RemoveAll(invoice => invoice.InvoiceNumber == record.InvoiceNumber);
            records.Add(record);
            records = records
                .OrderByDescending(invoice => invoice.Date)
                .ThenByDescending(invoice => invoice.InvoiceNumber)
                .ToList();

            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true }));
        }

        private void EnsureStoreExists()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }
    }
}
