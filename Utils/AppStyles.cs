using System;
using System.Drawing;
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
        }

        public static void ApplyTotalLabel(Label label, bool emphasized = false)
        {
            label.ForeColor = emphasized ? Primary : Ink;
            label.Font = emphasized ? new Font("Segoe UI Semibold", 12F) : TotalFont;
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
    }

    internal enum ButtonRole
    {
        Primary,
        Secondary,
        Accent
    }
}
