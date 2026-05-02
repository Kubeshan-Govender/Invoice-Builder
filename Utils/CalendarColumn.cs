using System.Globalization;
using System.Windows.Forms;
using System;

namespace InvoiceBuilder.Utils
{
    public class CalendarColumn : DataGridViewColumn
    {
        public CalendarColumn() : base(new CalendarCell())
        {
        }

        public override DataGridViewCell CellTemplate
        {
            get => base.CellTemplate;
            set
            {
                if (value is not null && !value.GetType().IsAssignableTo(typeof(CalendarCell)))
                {
                    throw new InvalidCastException("CellTemplate must be a CalendarCell.");
                }

                base.CellTemplate = value;
            }
        }
    }

    public class CalendarCell : DataGridViewTextBoxCell
    {
        public CalendarCell()
        {
            Style.Format = "yyyy/MM/dd";
        }

        public override void InitializeEditingControl(int rowIndex, object? initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);

            if (DataGridView?.EditingControl is CalendarEditingControl control)
            {
                control.Format = DateTimePickerFormat.Custom;
                control.CustomFormat = "yyyy/MM/dd";
                control.Value = ReadDate(Value);
            }
        }

        public override Type EditType => typeof(CalendarEditingControl);

        public override Type ValueType => typeof(DateTime);

        public override object DefaultNewRowValue => DateTime.Today;

        private static DateTime ReadDate(object? value)
        {
            if (value is DateTime date)
            {
                return date;
            }

            return DateTime.TryParse(value?.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsedDate)
                ? parsedDate
                : DateTime.Today;
        }
    }

    public class CalendarEditingControl : DateTimePicker, IDataGridViewEditingControl
    {
        private DataGridView? _dataGridView;
        private bool _valueChanged;
        private int _rowIndex;

        public object EditingControlFormattedValue
        {
            get => Value.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            set
            {
                if (DateTime.TryParse(value?.ToString(), out var parsedDate))
                {
                    Value = parsedDate;
                }
            }
        }

        public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
        {
            return EditingControlFormattedValue;
        }

        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
            Font = dataGridViewCellStyle.Font;
            CalendarForeColor = dataGridViewCellStyle.ForeColor;
            CalendarMonthBackground = dataGridViewCellStyle.BackColor;
        }

        public int EditingControlRowIndex
        {
            get => _rowIndex;
            set => _rowIndex = value;
        }

        public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey)
        {
            return (keyData & Keys.KeyCode) switch
            {
                Keys.Left or Keys.Up or Keys.Down or Keys.Right or Keys.Home or Keys.End or Keys.PageDown or Keys.PageUp => true,
                _ => !dataGridViewWantsInputKey
            };
        }

        public void PrepareEditingControlForEdit(bool selectAll)
        {
        }

        public bool RepositionEditingControlOnValueChange => false;

        public DataGridView? EditingControlDataGridView
        {
            get => _dataGridView;
            set => _dataGridView = value;
        }

        public bool EditingControlValueChanged
        {
            get => _valueChanged;
            set => _valueChanged = value;
        }

        public Cursor EditingPanelCursor => base.Cursor;

        protected override void OnValueChanged(EventArgs eventargs)
        {
            _valueChanged = true;
            _dataGridView?.NotifyCurrentCellDirty(true);
            base.OnValueChanged(eventargs);
        }
    }
}
