using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Core.Controls {
    public class GridControl : DataGridView {

        public GridControl() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.AutoGenerateColumns = false;
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;
            this.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.BackgroundColor = System.Drawing.SystemColors.Window;
            this.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.DefaultCellStyle = dataGridViewCellStyle1;
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridColor = System.Drawing.SystemColors.ControlLight;
            this.Location = new System.Drawing.Point(0, 25);
            this.MultiSelect = false;
            this.ReadOnly = true;
            this.RowHeadersWidth = (int)(24 * this.DeviceDpi / 96.0);
            this.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Size = new System.Drawing.Size(400, 300);
            this.TabIndex = 1;
            this.DoubleBuffered = true;
        }

        [Browsable(false)]
        public object SelectedItem {
            get {
                if (SelectedRows.Count > 0) {
                    try {
                        return SelectedRows[0].DataBoundItem;
                    } catch {
                        return null;
                    }
                }
                return null;
            }
            set {
                for (int i = 0; i < Rows.Count; i++) {
                    if (Equals(Rows[i].DataBoundItem, value)) {
                        Rows[i].Selected = true;
                        if (Rows[i].Cells[0].Visible) {
                            CurrentCell = Rows[i].Cells[0];
                        }
                        break;
                    }
                }
            }
        }

        [DefaultValue(null)]
        [AttributeProvider(typeof(IListSource))]
        public IList ItemsSource {
            get {
                return (IList)DataSource;
            }
            set {
                var oldItem = SelectedItem;
                string oldSortProperty = SortProperty;
                int oldColumnIndex = CurrentCell != null ? CurrentRow.Cells.IndexOf(CurrentCell) : 0;
                ListSortDirection oldSortDirection = SortDirection;
                DataSource = value;
                ApplySort(oldSortProperty, oldSortDirection);
                SelectedItem = oldItem;
                if (SelectedRows.Count > 0) {
                    if (SelectedRows[0].Cells[oldColumnIndex].Visible) {
                        CurrentCell = SelectedRows[0].Cells[oldColumnIndex];
                    }
                }
                SetupColumnsAutoSize();
            }
        }
        
        public string SortProperty {
            get {
                IBindingList list = ItemsSource as IBindingList;
                if (list == null || list.SortProperty == null || list.SortProperty.Name == null) {
                    return sortProperty;
                }
                return list.SortProperty.Name;
            }
            set {
                ApplySort(value, SortDirection);
            }
        }

        [DefaultValue(ListSortDirection.Ascending)]
        public ListSortDirection SortDirection {
            get {
                IBindingList list = ItemsSource as IBindingList;
                if (list == null || list.SortProperty == null) {
                    return sortDirection;
                }
                return list.SortDirection;
            }
            set {
                ApplySort(SortProperty, value);
            }
        }

        string sortProperty;
        ListSortDirection sortDirection;

        void ApplySort(string sortPropertyName, ListSortDirection direction) {
            if (string.IsNullOrEmpty(sortPropertyName)) {
                return;
            }
            var sortColumn = Columns[sortPropertyName];
            if (sortColumn != null) {
                Sort(sortColumn, direction);
            }
            sortProperty = sortPropertyName;
            sortDirection = direction;
        }

        public void AutoCreateColumns(Type itemType) {
            Columns.Clear();
            if (itemType == null) {
                return;
            }
            PropertyInfo[] properties = ReflectionHelper.GetVisibleProperties(itemType);
            if (properties.Length == 0) {
                throw new InvalidOperationException(string.Format("Для типа {0} не определены свойства для отображения с помощью атрибута DisplayAttribute.", itemType.Name));
            }
            foreach (var property in properties) {
                FieldVisibility vis = ReflectionHelper.GetVisibility(property);
                if (!vis.HasFlag(FieldVisibility.List)) {
                    continue;
                }
                Type type = property.PropertyType;
                if (type == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(type) || type == typeof(byte[])) {
                    var col = CreateColumn(property);
                    Columns.Add(col);
                }
            }
        }

        protected virtual DataGridViewColumn CreateColumn(PropertyInfo property) {
            Type type = property.PropertyType;
            DataGridViewColumn col = null;
            string format = ReflectionHelper.GetPropertyDisplayFormat(property);        
            if (type == typeof(decimal) || type == typeof(double) || type == typeof(float)
                || type == typeof(decimal?) || type == typeof(double?) || type == typeof(float?)) {
                col = new DataGridViewTextBoxColumn();
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
                if (!string.IsNullOrEmpty(format)) {
                    col.DefaultCellStyle.Format = format;
                }
            }
            if (type == typeof(int) || type == typeof(uint) || type == typeof(byte) || type == typeof(short) || type == typeof(long) || type == typeof(ulong)
                || type == typeof(int?) || type == typeof(uint?) || type == typeof(byte?) || type == typeof(short?) || type == typeof(long?) || type == typeof(ulong?)) {
                col = new DataGridViewTextBoxColumn();
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
                if (!string.IsNullOrEmpty(format)) {
                    col.DefaultCellStyle.Format = format;
                }
            }
            if (type == typeof(DateTime) || type == typeof(DateTime?)) {
                col = new DataGridViewTextBoxColumn();
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopCenter;
                col.ValueType = typeof(DateTime);
                if (string.IsNullOrEmpty(format)) {
                    col.DefaultCellStyle.Format = "dd.MM.yyyy";
                }
            }
            if (type == typeof(bool) || type == typeof(bool?)) {
                col = new DataGridViewCheckBoxColumn(false);
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopCenter;
            }
            if (type == typeof(byte[])) {
                col = new DataGridViewImageColumn();
                ((DataGridViewImageColumn)col).ImageLayout = DataGridViewImageCellLayout.Zoom;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopCenter;
            }
            if (col == null) {
                col = new DataGridViewTextBoxColumn();
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
                col.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            }
            col.HeaderText = ReflectionHelper.GetPropertyName(property);
            col.DataPropertyName = property.Name;
            col.Name = col.DataPropertyName;
            return col;
        }

        public void ApplyDefaultSort(Type itemType) {
            PropertyInfo[] properties = ReflectionHelper.GetVisibleProperties(itemType);
            bool sortApplied = false;
            foreach (var prop in properties) {
                SortDirectionAttribute sortAttr = ReflectionHelper.GetSortDirection(prop);
                if (sortAttr != null) {
                    ApplySort(prop.Name, sortAttr.Direction);
                    sortApplied = true;
                    break;
                }
            }
            if (!sortApplied) {
                PropertyInfo defaultProperty = ReflectionHelper.GetDefaultProperty(itemType);
                if (defaultProperty != null && properties.Contains(defaultProperty)) {
                    ApplySort(defaultProperty.Name, ListSortDirection.Ascending);
                }
            }
        }
        bool columnWidthInitialized = false;
        void SetupColumnsAutoSize() {
            if (Rows.Count < 1 || Columns.Count < 1) {
                return;
            }
            if(columnWidthInitialized) {
                return;
            }
            int[] colSizes = new int[Columns.Count];
            for (int c = 0; c < Columns.Count; c++) {
                int[] rowSizes = new int[Rows.Count];
                for (int r = 0; r < Rows.Count; r++) {
                    string text = Rows[r].Cells[c].FormattedValue?.ToString();
                    rowSizes[r] = text != null ? text.Length : 0;
                }
                Array.Sort(rowSizes);
                colSizes[c] = rowSizes[rowSizes.Length / 2];
            }
            int sumSize = colSizes.Sum();
            if (sumSize == 0) {
                return;
            }
            for (int i = 0; i < Columns.Count; i++) {
                Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                if (colSizes[i] > 0) {
                    Columns[i].FillWeight = colSizes[i] / (float)sumSize;
                } else {
                    Columns[i].FillWeight = 0.1f;
                }
                Columns[i].MinimumWidth = (int)(80 * this.DeviceDpi / 96.0);
            }
            columnWidthInitialized = true;
        }
    }
}
