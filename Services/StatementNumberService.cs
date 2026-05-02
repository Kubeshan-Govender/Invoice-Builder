using System;
using System.IO;

namespace InvoiceBuilder.Services
{
    public class StatementNumberService
    {
        private readonly string _filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "InvoiceBuilder",
            "statement_number.txt");

        public int GetNextStatementNumberPreview()
        {
            EnsureFileExists();
            return int.Parse(File.ReadAllText(_filePath)) + 1;
        }

        public void MarkStatementNumberUsed(int statementNumber)
        {
            EnsureFileExists();
            var currentNumber = int.Parse(File.ReadAllText(_filePath));
            if (statementNumber > currentNumber)
            {
                File.WriteAllText(_filePath, statementNumber.ToString());
            }
        }

        private void EnsureFileExists()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "0");
            }
        }
    }
}
