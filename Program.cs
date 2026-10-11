using System.Threading;
using QuestPDF.Infrastructure;

using System;
using System.Windows.Forms;
namespace InvoiceBuilder
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using var singleInstanceMutex = new Mutex(true, @"Local\InvoiceBuilder.SingleInstance", out var isFirstInstance);
            if (!isFirstInstance)
            {
                MessageBox.Show("Invoice Builder is already running.", "Invoice Builder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            QuestPDF.Settings.License = LicenseType.Community;
            ApplicationConfiguration.Initialize();

            using (var splash = new SplashForm())
            {
                splash.Show();
                splash.Refresh();
                Thread.Sleep(1100);
            }

            Application.Run(new InvoiceBuilder());
            }
            finally
            {
                singleInstanceMutex.ReleaseMutex();
            }
        }
    }
}
