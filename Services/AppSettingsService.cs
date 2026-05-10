using InvoiceBuilder.Models;
using System;
using System.IO;
using System.Text.Json;

namespace InvoiceBuilder.Services
{
    public class AppSettingsService
    {
        private readonly string _filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "InvoiceBuilder",
            "settings.json");

        public AppSettings Load()
        {
            EnsureStoreExists();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }

        public void Save(AppSettings settings)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }

        private void EnsureStoreExists()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

            if (!File.Exists(_filePath))
            {
                Save(new AppSettings());
            }
        }
    }
}
