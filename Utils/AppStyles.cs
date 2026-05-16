using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace InvoiceBuilder.Utils
{
    internal static class AppStyles
    {
        public static readonly Color Ink = Color.FromArgb(24, 34, 45);
        public static readonly Color MutedInk = Color.FromArgb(89, 101, 117);
        public static readonly Color Surface = Color.FromArgb(247, 249, 252);
        public static readonly Color Panel = Color.White;
        public static readonly Color Line = Color.FromArgb(214, 221, 231);
        public static readonly Color Primary = Color.FromArgb(18, 84, 129);
        public static readonly Color PrimaryHover = Color.FromArgb(13, 68, 105);
        public static readonly Color Accent = Color.FromArgb(205, 151, 54);
        public static readonly Color Header = Color.FromArgb(31, 45, 61);

        private static readonly Font BaseFont = new("Segoe UI", 9F);
        private static readonly Font LabelFont = new("Segoe UI Semibold", 9F);
        private static readonly Font TotalFont = new("Segoe UI Semibold", 10F);

        public static void ApplyForm(Form form)
        {
            form.BackColor = Surface;
            form.Font = BaseFont;
            form.ForeColor = Ink;
            form.Icon = LoadAppIcon();
            form.StartPosition = form.StartPosition == FormStartPosition.WindowsDefaultLocation
                ? FormStartPosition.CenterScreen
                : form.StartPosition;

            foreach (Control control in form.Controls)
            {
                ApplyControl(control);
            }
        }

        public static void ApplyControl(Control control)
        {
            switch (control)
            {
                case TableLayoutPanel table:
                    table.BackColor = Surface;
                    break;
                case FlowLayoutPanel flow:
                    flow.BackColor = Surface;
                    flow.Padding = new Padding(0, 6, 0, 0);
                    break;
                case Label label:
                    label.ForeColor = MutedInk;
                    label.Font = LabelFont;
                    label.Margin = new Padding(4, 4, 4, 4);
                    break;
                case TextBox textBox:
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    textBox.BackColor = Panel;
                    textBox.ForeColor = Ink;
                    textBox.Margin = new Padding(4, 3, 4, 6);
                    break;
                case ComboBox combo:
                    combo.FlatStyle = FlatStyle.Flat;
                    combo.BackColor = Panel;
                    combo.ForeColor = Ink;
                    combo.Margin = new Padding(4, 3, 4, 6);
                    break;
                case DateTimePicker picker:
                    picker.CalendarTitleBackColor = Primary;
                    picker.CalendarTitleForeColor = Color.White;
                    picker.Margin = new Padding(4, 3, 4, 6);
                    break;
                case DataGridView grid:
                    ApplyGrid(grid);
                    break;
                case Button button:
                    ApplyButton(button, ButtonRole.Secondary);
                    break;
            }

            foreach (Control child in control.Controls)
            {
                ApplyControl(child);
            }
        }

        public static void ApplyGrid(DataGridView grid)
        {
            grid.BackgroundColor = Panel;
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = Line;
            grid.RowHeadersVisible = false;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Header;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Header;
            grid.ColumnHeadersHeight = 34;
            grid.DefaultCellStyle.BackColor = Panel;
            grid.DefaultCellStyle.ForeColor = Ink;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(221, 235, 247);
            grid.DefaultCellStyle.SelectionForeColor = Ink;
            grid.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(251, 252, 254);
            grid.RowTemplate.Height = 30;
        }

        public static void ApplyButton(Button button, ButtonRole role)
        {
            var backColor = role switch
            {
                ButtonRole.Primary => Primary,
                ButtonRole.Accent => Accent,
                _ => Color.FromArgb(232, 237, 244)
            };

            var foreColor = role == ButtonRole.Secondary ? Ink : Color.White;

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = role == ButtonRole.Secondary ? 1 : 0;
            button.FlatAppearance.BorderColor = Line;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Font = new Font("Segoe UI Semibold", 9F);
            button.Height = Math.Max(button.Height, 34);
            button.Padding = new Padding(10, 0, 10, 0);
            button.Margin = new Padding(6, 3, 0, 3);
            button.Cursor = Cursors.Hand;
            button.ImageAlign = ContentAlignment.MiddleLeft;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.TextImageRelation = TextImageRelation.ImageBeforeText;
        }

        public static void ApplyButtonIcon(Button button, ButtonIcon icon, ButtonRole role)
        {
            var iconColor = role == ButtonRole.Secondary ? Ink : Color.White;
            button.Image = CreateButtonIcon(icon, iconColor);
            button.ImageAlign = ContentAlignment.MiddleLeft;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.TextImageRelation = TextImageRelation.ImageBeforeText;
            button.Padding = new Padding(12, 0, 14, 0);
        }

        public static void ApplyTotalLabel(Label label, bool emphasized = false)
        {
            label.ForeColor = emphasized ? Primary : Ink;
            label.Font = emphasized ? new Font("Segoe UI Semibold", 14F) : new Font("Segoe UI Semibold", 11F);
        }

        public static void ApplyHeaderPanel(TableLayoutPanel panel)
        {
            panel.BackColor = Color.FromArgb(236, 242, 248);
            panel.Padding = new Padding(10, 8, 10, 8);
        }

        public static Icon LoadAppIcon()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Media", "icon.ico");
            if (!File.Exists(path))
            {
                path = Path.Combine("Media", "icon.ico");
            }

            return File.Exists(path) ? new Icon(path) : null;
        }

        private static Bitmap CreateButtonIcon(ButtonIcon icon, Color color)
        {
            var bitmap = new Bitmap(18, 18);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);

            using var pen = new Pen(color, 1.8F)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };

            using var brush = new SolidBrush(color);

            switch (icon)
            {
                case ButtonIcon.Generate:
                    DrawDocumentIcon(graphics, pen, color);
                    graphics.DrawLine(pen, 6, 12, 8, 14);
                    graphics.DrawLine(pen, 8, 14, 13, 8);
                    break;
                case ButtonIcon.Calculate:
                    using (var font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Pixel))
                    {
                        graphics.DrawString("Σ", font, brush, 3, 1);
                    }
                    break;
                case ButtonIcon.Add:
                    graphics.DrawLine(pen, 9, 4, 9, 14);
                    graphics.DrawLine(pen, 4, 9, 14, 9);
                    break;
                case ButtonIcon.Delete:
                    graphics.DrawLine(pen, 5, 6, 13, 6);
                    graphics.DrawLine(pen, 7, 6, 7, 14);
                    graphics.DrawLine(pen, 11, 6, 11, 14);
                    graphics.DrawRectangle(pen, 6, 6, 6, 9);
                    graphics.DrawLine(pen, 7, 4, 11, 4);
                    break;
                case ButtonIcon.Statement:
                    graphics.DrawRectangle(pen, 4, 5, 9, 10);
                    graphics.DrawLine(pen, 7, 3, 14, 3);
                    graphics.DrawLine(pen, 14, 3, 14, 12);
                    graphics.DrawLine(pen, 7, 8, 11, 8);
                    graphics.DrawLine(pen, 7, 11, 11, 11);
                    break;
                case ButtonIcon.Settings:
                    graphics.DrawEllipse(pen, 5, 5, 8, 8);
                    graphics.DrawEllipse(pen, 8, 8, 2, 2);
                    for (var i = 0; i < 8; i++)
                    {
                        var angle = i * Math.PI / 4;
                        var x1 = 9 + Math.Cos(angle) * 5;
                        var y1 = 9 + Math.Sin(angle) * 5;
                        var x2 = 9 + Math.Cos(angle) * 7;
                        var y2 = 9 + Math.Sin(angle) * 7;
                        graphics.DrawLine(pen, (float)x1, (float)y1, (float)x2, (float)y2);
                    }
                    break;
            }

            return bitmap;
        }

        private static void DrawDocumentIcon(Graphics graphics, Pen pen, Color color)
        {
            using var path = new GraphicsPath();
            path.AddLine(5, 3, 11, 3);
            path.AddLine(11, 3, 14, 6);
            path.AddLine(14, 6, 14, 15);
            path.AddLine(14, 15, 5, 15);
            path.CloseFigure();
            graphics.DrawPath(pen, path);
            graphics.DrawLine(pen, 11, 3, 11, 6);
            graphics.DrawLine(pen, 11, 6, 14, 6);
            graphics.DrawLine(pen, 7, 8, 12, 8);
        }
    }

    internal enum ButtonRole
    {
        Primary,
        Secondary,
        Accent
    }

    internal enum ButtonIcon
    {
        Generate,
        Calculate,
        Add,
        Delete,
        Statement,
        Settings
    }
}
