using System;
using System.Drawing;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InvoiceBuilder
{
    internal sealed class LoadingDialog : Form
    {
        private LoadingDialog(string message)
        {
            Text = "Please wait";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            ControlBox = false;
            ShowInTaskbar = false;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(390, 132);
            Font = new Font("Segoe UI", 10F);

            var label = new Label
            {
                Text = message,
                Dock = DockStyle.Top,
                Height = 62,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(18, 10, 18, 4)
            };
            var progress = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Height = 20,
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 28,
                Margin = new Padding(20)
            };

            Controls.Add(progress);
            Controls.Add(label);
        }

        public static void Run(Form owner, string message, Func<Task> operation)
        {
            Exception failure = null;
            using var dialog = new LoadingDialog(message);
            dialog.Shown += async (_, _) =>
            {
                try
                {
                    await operation();
                }
                catch (Exception exception)
                {
                    failure = exception;
                }
                finally
                {
                    dialog.Close();
                }
            };

            dialog.ShowDialog(owner);
            if (failure is not null)
            {
                ExceptionDispatchInfo.Capture(failure).Throw();
            }
        }
    }
}
