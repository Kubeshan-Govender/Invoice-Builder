using InvoiceBuilder.Utils;
using System.Drawing;
using System.Windows.Forms;

namespace InvoiceBuilder
{
    internal sealed class SplashForm : Form
    {
        public SplashForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = AppStyles.Surface;
            ClientSize = new Size(360, 230);
            ShowInTaskbar = false;
            Icon = AppStyles.LoadAppIcon();

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = AppStyles.Surface,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(28)
            };

            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var icon = AppStyles.LoadAppIcon();
            var picture = new PictureBox
            {
                Image = icon?.ToBitmap(),
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill
            };

            var title = new Label
            {
                Text = "Faith Trucking",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Semibold", 16F),
                ForeColor = AppStyles.Primary
            };

            var subtitle = new Label
            {
                Text = "Invoice Builder",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10F),
                ForeColor = AppStyles.MutedInk
            };

            var accent = new Panel
            {
                Height = 3,
                Width = 120,
                BackColor = AppStyles.Accent,
                Anchor = AnchorStyles.Top
            };

            panel.Controls.Add(picture, 0, 0);
            panel.Controls.Add(title, 0, 1);
            panel.Controls.Add(subtitle, 0, 2);
            panel.Controls.Add(accent, 0, 3);
            Controls.Add(panel);
        }
    }
}
