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
    class ComboBoxControl : Control {

        readonly ComboBox comboBox = new ComboBox();
        readonly Button ellipsisButton = new Button();
        readonly Button clearButton = new Button();

        public ComboBoxControl() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            BackColor = SystemColors.Control;

            comboBox.Top = 0;
            comboBox.Left = 0;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.Visible = true;
            comboBox.SelectedIndexChanged += OnSelectedItemChanged;
            Controls.Add(comboBox);

            ellipsisButton.Text = "...";
            ellipsisButton.Width = (int)(24 * this.DeviceDpi / 96.0);
            ellipsisButton.Height = comboBox.Height;
            ellipsisButton.Top = -1;
            ellipsisButton.Visible = true;
            ellipsisButton.Click += OnEllipsisClick;
            Controls.Add(ellipsisButton);

            clearButton.Text = "X";
            clearButton.Top = -1;
            clearButton.Width = (int)(24 * this.DeviceDpi / 96.0);
            clearButton.Height = comboBox.Height;
            clearButton.Visible = true;
            clearButton.Click += OnClearClick;
            Controls.Add(clearButton);

            SetupControlsLocations();
        }

        private void OnSelectedItemChanged(object sender, EventArgs e) {
            if (SelectedItemChanged != null) {
                SelectedItemChanged(this, EventArgs.Empty);
            }
        }

        private void OnClearClick(object sender, EventArgs e) {
            SelectedItem = null;
        }

        private void OnEllipsisClick(object sender, EventArgs e) {
            if (EllipsisClick != null) {
                EllipsisClick(this, EventArgs.Empty);
            }
        }

        void SetupControlsLocations() {
            int rightPos = Width;
            if (EllipsisButtonVisible) {
                ellipsisButton.Left = rightPos - ellipsisButton.Width;
                rightPos = ellipsisButton.Left - 2;
            }
            if (ClearButtonVisible) {
                clearButton.Left = rightPos - clearButton.Width;
                rightPos = clearButton.Left - 2;
            }
            comboBox.Width = rightPos;
        }

        protected override Size DefaultSize {
            get {
                return new Size(200, comboBox.Height);
            }
        }

        protected override Size DefaultMinimumSize {
            get {
                return new Size(40, comboBox.Height);
            }
        }

        protected override Size DefaultMaximumSize {
            get {
                return new Size(0, comboBox.Height);
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

        bool ellipsisButtonVisible = true;

        [DefaultValue(true)]
        public bool EllipsisButtonVisible {
            get {
                return ellipsisButtonVisible;
            }
            set {
                ellipsisButtonVisible = ellipsisButton.Visible = value;
                SetupControlsLocations();
            }
        }

        [DefaultValue(false)]
        public bool ReadOnly {
            get {
                return !comboBox.Enabled;
            }
            set {
                comboBox.Enabled = clearButton.Enabled = !value;
            }
        }

        [DefaultValue(null)]
        [AttributeProvider(typeof(IListSource))]
        public object ItemsSource {
            get {
                return comboBox.DataSource;
            }
            set {
                if (comboBox.DataSource != value) {
                    object oldValue = SelectedItem;
                    comboBox.DataSource = value;
                    SelectedItem = oldValue;
                }
            }
        }

        [DefaultValue("")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string DisplayMember {
            get {
                return comboBox.DisplayMember;
            }
            set {
                comboBox.DisplayMember = value;
            }
        }

        [DefaultValue("")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ValueMember {
            get {
                return comboBox.ValueMember;
            }
            set {
                comboBox.ValueMember = value;
            }
        }

        [Bindable(true)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedItem {
            get {
                return comboBox.SelectedItem;
            }
            set {
                comboBox.SelectedItem = value;
                if (value == null || value == DBNull.Value) {
                    comboBox.SelectedIndex = -1;
                }
            }
        }

        public event EventHandler EllipsisClick;
        public event EventHandler SelectedItemChanged;
    }
}
