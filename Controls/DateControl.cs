using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Core.Controls {
    class DateControl : Control {

        readonly NumericUpDown dayBox = new NumericUpDown();
        readonly ComboBox monthBox = new ComboBox();
        readonly NumericUpDown yearBox = new NumericUpDown();
        readonly Button clearButton = new Button();
        readonly TextBox txbDate = new TextBox();

        public DateControl() {
            InitializeComponent();
        }

        private void InitializeComponent() {

            txbDate.ReadOnly = true;
            Controls.Add(txbDate);
            txbDate.Visible = false;

            dayBox.Top = 0;
            dayBox.Maximum = 31;
            dayBox.Minimum = 0;
            dayBox.Width = 50;
            Controls.Add(dayBox);
            dayBox.Visible = true;
            dayBox.ValueChanged += OnValueChanged;

            monthBox.Top = 0;
            monthBox.Left = 0;
            monthBox.DropDownStyle = ComboBoxStyle.DropDownList;
            monthBox.Visible = true;
            monthBox.Items.AddRange(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthGenitiveNames);
            Controls.Add(monthBox);
            monthBox.SelectedIndexChanged += OnMonthChanged;

            yearBox.Top = 0;
            yearBox.Maximum = 2100;
            yearBox.Minimum = 0;
            yearBox.Width = (int)(50 * this.DeviceDpi / 96.0);
            Controls.Add(yearBox);
            yearBox.Visible = true;
            yearBox.ValueChanged += OnValueChanged;

            clearButton.Text = "X";
            clearButton.Top = -1;
            clearButton.Width = (int)(24 * this.DeviceDpi / 96.0);
            clearButton.Height = yearBox.Height;
            clearButton.Visible = true;
            clearButton.Click += OnClearClick;
            Controls.Add(clearButton);

            Value = DateTime.Today;
            SetupControlsLocations();
        }

        bool suppressValueChanged;
        private void OnValueChanged(object sender, EventArgs e) {
            if (ValueChanged != null && !suppressValueChanged) {
                ValueChanged(this, EventArgs.Empty);
            }
        }
        private void OnMonthChanged(object sender, EventArgs e) {
            bool oldSuppressValueChanged = suppressValueChanged;
            suppressValueChanged = true;
            try {
                if (monthBox.SelectedIndex >= 0 && monthBox.SelectedIndex < 12) {
                    if (dayBox.Value == 0) {
                        dayBox.Value = 1;
                    }
                    if (yearBox.Value == 0) {
                        yearBox.Value = DateTime.Today.Year;
                    }
                } else {
                    dayBox.Value = 0;
                    yearBox.Value = 0;
                    dayBox.ResetText();
                    yearBox.ResetText();
                }
            } finally {
                suppressValueChanged = oldSuppressValueChanged;
            }
            if (ValueChanged != null && !suppressValueChanged) {
                ValueChanged(this, EventArgs.Empty);
            }
        }

        private void OnClearClick(object sender, EventArgs e) {
            Value = DateTime.MinValue;            
        }

        void SetupControlsLocations() {
            int rightPos = Width;
            if (ClearButtonVisible) {
                clearButton.Left = rightPos - clearButton.Width;
                rightPos = clearButton.Left - 2;
            }
            yearBox.Left = rightPos - yearBox.Width;
            rightPos = yearBox.Left - 2;
            monthBox.Left = dayBox.Right + 2;
            monthBox.Width = rightPos - monthBox.Left;
            txbDate.Width = Width;
        }

        protected override Size DefaultSize {
            get {
                return new Size(200, dayBox.Height);
            }
        }

        protected override Size DefaultMinimumSize {
            get {
                return new Size(40, dayBox.Height);
            }
        }

        protected override Size DefaultMaximumSize {
            get {
                return new Size(0, dayBox.Height);
            }
        }

        protected override void OnResize(EventArgs e) {
            base.OnResize(e);
            SetupControlsLocations();
        }

        bool clearButtonVisible = true;

        [DefaultValue(true)]
        public bool ClearButtonVisible {
            get {
                return clearButtonVisible;
            }
            set {
                clearButtonVisible = clearButton.Visible = value;
                SetupControlsLocations();
            }
        }

        [DefaultValue(false)]
        public bool ReadOnly {
            get {
                return !monthBox.Visible;
            }
            set {
                dayBox.Visible = monthBox.Visible = yearBox.Visible = clearButton.Visible = !value;
                txbDate.Visible = value;
                SetupControlsLocations();
            }
        }

        [Bindable(true)]
        public DateTime Value {
            get {
                if (yearBox.Value != 0 && dayBox.Value != 0 && monthBox.SelectedIndex >= 0 && monthBox.SelectedIndex < 12) {
                    try {
                        return new DateTime((int)yearBox.Value, monthBox.SelectedIndex + 1, (int)dayBox.Value);
                    } catch { }
                }
                return DateTime.MinValue;
            }
            set {
                suppressValueChanged = true;
                if (value != DateTime.MinValue) {
                    yearBox.Value = value.Year;
                    dayBox.Value = value.Day;
                    monthBox.SelectedIndex = value.Month - 1;
                    txbDate.Text = value.ToString("dd.MM.yyyy");
                } else {
                    yearBox.Value = 0;
                    dayBox.Value = 0;                    
                    monthBox.SelectedIndex = -1;
                    dayBox.ResetText();
                    yearBox.ResetText();
                    txbDate.Text = "";
                }
                suppressValueChanged = false;
                if (ValueChanged != null) {
                    ValueChanged(this, EventArgs.Empty);
                }
            }
        }

        [Browsable(false)]
        public bool IsInvalidDate {
            get {
                if (yearBox.Value == 0 && dayBox.Value == 0 && (monthBox.SelectedIndex < 0 || monthBox.SelectedIndex >= 12)) {
                    return true;
                }
                try {
                    DateTime date = new DateTime((int)yearBox.Value, monthBox.SelectedIndex + 1, (int)dayBox.Value);
                    return (date != DateTime.MinValue);
                } catch {
                    return false;
                }
            }
        }

        [Browsable(false)]
        public bool IsEmpty {
            get {
                return (yearBox.Value == 0 && dayBox.Value == 0 && (monthBox.SelectedIndex < 0 || monthBox.SelectedIndex >= 12));
            }
        }

        public event EventHandler ValueChanged;
    }
}
