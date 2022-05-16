using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Core.Controls;

namespace Core.Windows {
    public partial class EditorWindow : Form {

        protected readonly bool IsNewItem;
        protected DataObjectBase BusinessObject;
        protected readonly DbContext dbContext;
        protected readonly Dictionary<string, Control> EditorControls = new Dictionary<string, Control>();

        const int ControlMargin = 2;
        const int ControlSpacing = 4;
        const int DefaultGridHeight = 300;
        const int MinWidth = 300;

        public EditorWindow(DbContext dbContext, DataObjectBase item) {
            this.dbContext = dbContext;
            this.IsNewItem = (ReflectionHelper.GetObjectID(item) <= 0);
            this.BusinessObject = item;
            BusinessObject.PropertyChanged += OnBusinessObjectChanged;
            InitializeComponent();
            CreateControls();
            SetupSecurity();
            Text = GetWindowTitle();
            Height = MinimumSize.Height;
        }        

        public EditorWindow() {
            InitializeComponent();
        }

        int NonClientHeight {
            get {
                return Height - ClientSize.Height + (int)(16 * this.DeviceDpi / 96.0);
            }
        }

        protected virtual void SetupSecurity() {
            bool? tmp = SecurityHelper.IsEditOrCreateAllowed(BusinessObject.GetType());
            bool isEditAllowed = LiftToTrue(tmp);
            if (tmp == null) {
                PropertyInfo[] props = ReflectionHelper.GetVisibleProperties(BusinessObject);
                foreach (var prop in props) {
                    tmp = SecurityHelper.IsEditAllowed(prop);
                    if (LiftToTrue(tmp)) {
                        isEditAllowed = true;
                        break;
                    }
                }
            }
            btnOk.Enabled = isEditAllowed;
        }

        protected virtual void CreateControls() {
            PropertyInfo[] props = ReflectionHelper.GetVisibleProperties(BusinessObject);
            if (props.Length == 0) {
                throw new InvalidOperationException(string.Format("Для типа {0} не определены свойства для отображения с помощью атрибута DisplayAttribute.", BusinessObject.GetType().Name));
            }
            List<string> tabTitles = new List<string>();
            foreach (var prop in props) {
                string tabTitle = ReflectionHelper.GetPropertyTabTitle(prop);
                if (!tabTitles.Contains(tabTitle)) {
                    tabTitles.Add(tabTitle);
                }
            }
            if (tabTitles.Count == 1 && tabTitles[0] == null) {
                TableLayoutPanel grid = CreateControls(props);
                contentPanel.Controls.Add(grid);
                grid.Dock = DockStyle.Fill;
                MinimumSize = new Size(MinWidth, grid.MinimumSize.Height + buttonsPanel.Height + NonClientHeight + contentPanel.Padding.All * 2);                
            } else {
                int maxGridHeight = 0;
                TabControl tabControl = new TabControl();
                foreach (string tabTitle in tabTitles) {
                    TabPage tabItem = new TabPage();
                    tabItem.Text = tabTitle;
                    PropertyInfo[] propsForTab = props.Where(t => ReflectionHelper.GetPropertyTabTitle(t) == tabTitle).ToArray();
                    TableLayoutPanel grid = CreateControls(propsForTab);
                    grid.Padding = new Padding((int)(8 * this.DeviceDpi / 96.0));
                    tabItem.Controls.Add(grid);
                    grid.Dock = DockStyle.Fill;
                    tabControl.TabPages.Add(tabItem);
                    maxGridHeight = Math.Max(grid.MinimumSize.Height + grid.Padding.All * 2, maxGridHeight);
                }
                contentPanel.Controls.Add(tabControl);
                tabControl.Dock = DockStyle.Fill;
                MinimumSize = new Size(MinWidth, maxGridHeight + buttonsPanel.Height + NonClientHeight + contentPanel.Padding.All * 2);
            }
        }

        TableLayoutPanel CreateControls(PropertyInfo[] properties) {
            TableLayoutPanel grid = new TableLayoutPanel();
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, (int)(24 * this.DeviceDpi / 96.0)));

            List<Control> controls = new List<Control>();
            List<Control> labels = new List<Control>();
            List<Control> tabItems = new List<Control>();
            foreach (var property in properties) {
                FieldVisibility vis = ReflectionHelper.GetVisibility(property);
                if (!vis.HasFlag(FieldVisibility.Form)) {
                    continue;
                }
                var newGroupAttr = ReflectionHelper.GetAttribute<BeginNewGroupAttribute>(property);
                if (newGroupAttr != null) {
                    Control groupLabel = CreateGroupLabel(newGroupAttr.Title);
                    groupLabel.Dock = DockStyle.Top;
                    controls.Add(null);
                    labels.Add(groupLabel);
                }
                Control control = CreateControl(property);
                if (control != null) {
                    bool isSeparateTab = IsPlaceOnSeparateTab(property);
                    if (!isSeparateTab) {
                        if (!(control is Label)) {
                            control.Margin = new Padding(ControlMargin);
                        }
                        controls.Add(control);
                        control.Dock = DockStyle.Fill;
                        Control label = CreateLabel(property);
                        label.Dock = DockStyle.Top;                        
                        labels.Add(label);
                    } else {
                        Control tabItem = CreateTabItem(property);
                        tabItem.Controls.Add(control);
                        control.Dock = DockStyle.Fill;
                        tabItems.Add(tabItem);
                    }
                    EditorControls[property.Name] = control;
                }
            }

            for (int i = 0; i < controls.Count; i++) {
                Control control = controls[i];
                Control label = labels[i];
                if (control != null) {
                    //элемент редактирования данных
                    RowStyle rowDef = new RowStyle(SizeType.AutoSize);
                    grid.RowStyles.Insert(grid.RowStyles.Count - 1, rowDef);
                    grid.Controls.Add(label, 0, grid.RowStyles.Count - 2);
                    grid.Controls.Add(control, 1, grid.RowStyles.Count - 2);
                } else {
                    //заголовок группы
                    RowStyle rowDef = new RowStyle(SizeType.AutoSize);
                    grid.RowStyles.Insert(grid.RowStyles.Count - 1, rowDef);
                    grid.Controls.Add(label, 0, grid.RowStyles.Count - 2);
                    grid.SetColumnSpan(label, 2);
                }
            }

            if (tabItems.Count > 0) {
                RowStyle rowDef = new RowStyle(SizeType.AutoSize);
                grid.RowStyles.Insert(grid.RowStyles.Count - 1, rowDef);
                Control tabControl = CreateTabControl();
                grid.Controls.Add(tabControl, 0, grid.RowStyles.Count - 2);
                grid.SetColumnSpan(tabControl, 2);
                tabControl.Dock = DockStyle.Fill;
                for (int i = 0; i < tabItems.Count; i++) {
                    (tabControl as TabControl).TabPages.Add(tabItems[i] as TabPage);
                }
            } else {
                RowStyle rowDef = new RowStyle(SizeType.AutoSize);
                grid.RowStyles.Insert(grid.RowStyles.Count - 1, rowDef);
            }

            if (labels.Count > 0) {
                grid.ColumnStyles[0].SizeType = SizeType.Absolute;
                grid.ColumnStyles[0].Width = labels.Max(t => GetLabelWidth(t) * 11 / 10);
            }

            int totalControlsHeight = controls.Sum(t => (t != null ? t.Height : (int)(24 * this.DeviceDpi / 96.0)) + (int)(ControlSpacing * this.DeviceDpi / 96.0));
            grid.MinimumSize = new Size(0, totalControlsHeight + ((tabItems.Count > 0) ? (int)(DefaultGridHeight * this.DeviceDpi / 96.0) : 0));
            return grid;
        }

        int GetLabelWidth(Control label) {
            using (Graphics gr = this.CreateGraphics()) {
                return (int)gr.MeasureString(label.Text, label.Font).Width + label.Margin.Horizontal;
            }
        }

        protected virtual string GetWindowTitle() {
            if (dbContext != null) {
                if (IsNewItem) {
                    return string.Format("{0} - [новая запись]", GetBusinessObjectName());
                } else {
                    return string.Format("{0} - [редактирование]", GetBusinessObjectName());
                }
            } else {
                return ReflectionHelper.GetTypeName(BusinessObject);
            }
        }

        protected virtual string GetBusinessObjectName() {
            return ReflectionHelper.GetTypeName(BusinessObject);
        }

        protected virtual Control CreateControl(PropertyInfo property) {
            Type type = property.PropertyType;
            if (type == typeof(string)) {
                return CreateTextBox(property);
            }
            if (type == typeof(decimal) || type == typeof(double) || type == typeof(float)
                || type == typeof(decimal?) || type == typeof(double?) || type == typeof(float?)) {
                return CreateNumericUpDown(property, true);
            }
            if (type == typeof(int) || type == typeof(uint) || type == typeof(byte) || type == typeof(short) || type == typeof(long) || type == typeof(ulong)
                || type == typeof(int?) || type == typeof(uint?) || type == typeof(byte?) || type == typeof(short?) || type == typeof(long?) || type == typeof(ulong?)) {
                return CreateNumericUpDown(property, false);
            }
            if (type == typeof(DateTime) || type == typeof(DateTime?)) {
                return CreateDateTimePicker(property);
            }
            if (type == typeof(bool) || type == typeof(bool?)) {
                return CreateCheckBox(property);
            }
            if (typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType) {
                return CreateGrid(property);
            } else if (!type.IsValueType) {
                if (!type.IsArray) {
                    return CreateComboBox(property);
                } else if (type == typeof(byte[])) {
                    return CreateImageEdit(property);
                }
            }
            throw new NotSupportedException();
        }        

        protected bool IsPlaceOnSeparateTab(PropertyInfo property) {
            Type type = property.PropertyType;
            if (typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType) {
                return true;
            }
            return false;
        }

        protected virtual Control CreateGroupLabel(string groupTitle) {
            var control = new Label();
            control.TextAlign = ContentAlignment.MiddleLeft;
            control.Text = groupTitle;
            control.Font = new Font(control.Font, FontStyle.Bold);
            return control;
        }

        protected virtual Control CreateLabel(PropertyInfo property) {
            var control = new Label();
            control.TextAlign = ContentAlignment.MiddleLeft;            
            control.Text = ReflectionHelper.GetPropertyName(property);
            if (ReflectionHelper.IsPropertyRequired(property)) {
                control.Text += "*";
            }
            control.Text += ":";
            return control;
        }

        protected virtual Control CreateTextBox(PropertyInfo property) {
            bool isReadOnly = ReflectionHelper.IsPropertyReadonly(property);
            TextBox control = new TextBox();
            int maxLength = ReflectionHelper.GetPropertyTextMaxLength(property);
            if (maxLength > 0) {
                control.MaxLength = maxLength;
                if (maxLength > 255) {
                    control.Multiline = true;
                    control.AcceptsReturn = true;
                    control.ScrollBars = ScrollBars.Vertical;
                    control.Height = (int)(80 * this.DeviceDpi / 96.0);
                }
            }
            control.ReadOnly = isReadOnly;
            SetupBinding(control, nameof(TextBox.Text), property);
            return control;
        }

        protected virtual Control CreateNumericUpDown(PropertyInfo property, bool allowFractional) {
            bool isReadOnly = ReflectionHelper.IsPropertyReadonly(property);
            if (isReadOnly) {
                return CreateTextBox(property);
            }
            NumericUpDown control = new NumericUpDown();
            control.Minimum = decimal.MinValue;
            control.Maximum = decimal.MaxValue;
            control.ReadOnly = ReflectionHelper.IsPropertyReadonly(property);
            if (allowFractional) {
                control.DecimalPlaces = 2;
                string format = ReflectionHelper.GetPropertyDisplayFormat(property);
                if (format != null && format.StartsWith("F")) {
                    format = format.Remove(0, 1);
                    control.DecimalPlaces = Int32.Parse(format);
                }
            } else {
                control.DecimalPlaces = 0;
            }
            SetupBinding(control, nameof(NumericUpDown.Value), property);
            return control;
        }

        protected virtual Control CreateCheckBox(PropertyInfo property) {
            CheckBox control = new CheckBox();
            control.Enabled = !ReflectionHelper.IsPropertyReadonly(property);
            SetupBinding(control, nameof(CheckBox.Checked), property);
            return control;
        }

        protected virtual Control CreateComboBox(PropertyInfo property) {
            Control editorControl;
            bool isRequired = ReflectionHelper.IsPropertyRequired(property);
            bool isReadOnly = ReflectionHelper.IsPropertyReadonly(property);
            bool? isAllowEditList = SecurityHelper.IsEditOrCreateAllowed(property.PropertyType);
            if (isAllowEditList == null) {
                isAllowEditList = true;
            }
            if (isReadOnly) {
                isAllowEditList = false;
            }
            if (!isReadOnly) {
                ComboBoxControl control = new ComboBoxControl();
                List<DataObjectBase> items = GetCollectionFromDbContext(property.PropertyType);
                control.ItemsSource = items.OrderBy(t => t.ToString()).ToList();
                if (items.Count > 10) {
                    //
                }
                control.EllipsisButtonVisible = true;
                control.ClearButtonVisible = !isRequired;
                control.EllipsisClick += ((sender, e) => {
                    DataObjectBase selectedObject = UIHelper.EditCollection(property.PropertyType, this, true);
                    if (!isReadOnly) {
                        control.ItemsSource = GetCollectionFromDbContext(property.PropertyType).OrderBy(t => t.ToString()).ToList();
                    }
                    if (selectedObject != null) {
                        IEnumerable controlItems = control.ItemsSource as IEnumerable;
                        if (controlItems != null) {
                            foreach (var t in controlItems) {
                                if (t.Equals(selectedObject)) {
                                    control.SelectedItem = t;
                                }
                            }
                        }
                    }
                });
                Binding binding = SetupBinding(control, nameof(ComboBoxControl.SelectedItem), property);
                editorControl = control;
            } else {
                editorControl = CreateTextBox(property);
            }
            return editorControl;
        }

        protected virtual Control CreateDateTimePicker(PropertyInfo property) {
            string format = ReflectionHelper.GetPropertyDisplayFormat(property);
            if (ReflectionHelper.IsPropertyNullable(property)) {
                if (string.IsNullOrEmpty(format) || !format.Contains("HH")) {
                    DateControl control = new DateControl();
                    control.ReadOnly = ReflectionHelper.IsPropertyReadonly(property);
                    control.MaximumSize = new Size((int)(250 * this.DeviceDpi / 96.0), control.MaximumSize.Height);
                    Binding binding = SetupBinding(control, nameof(DateControl.Value), property);
                    binding.NullValue = DateTime.MinValue;
                    return control;
                } else {
                    throw new NotImplementedException("Редактирование даты/времени не поддерживается.");
                }
            } else {
                DateTimePicker control = new DateTimePicker();
                control.Enabled = !ReflectionHelper.IsPropertyReadonly(property);
                control.MaximumSize = new Size((int)(250 * this.DeviceDpi / 96.0), control.MaximumSize.Height);
                Binding binding = SetupBinding(control, nameof(DateTimePicker.Value), property);
                binding.NullValue = DateTime.MinValue;
                control.Format = DateTimePickerFormat.Custom;
                if (!string.IsNullOrEmpty(format)) {
                    control.CustomFormat = format;
                } else {
                    control.CustomFormat = "dd.MM.yyyy";
                }
                return control;
            }

        }

        protected virtual Control CreateImageEdit(PropertyInfo property) {
            var control = new ImageEditUserControl();
            control.ReadOnly = ReflectionHelper.IsPropertyReadonly(property);
            control.MaxBlobSize = ReflectionHelper.GetPropertyTextMaxLength(property);
            SetupBinding(control, nameof(ImageEditUserControl.BlobData), property);
            return control;
        }

        protected virtual Control CreateGrid(PropertyInfo property) {
            Type gridItemType = property.PropertyType.GenericTypeArguments.FirstOrDefault();
            GridUserControl control = new GridUserControl();
            control.AutoCreateColumns(gridItemType);
            control.SetupSecurity(gridItemType);
            SetupBinding(control, nameof(GridUserControl.ItemsSource), property);
            control.MinimumSize = new Size(0, (int)(80 * this.DeviceDpi / 96.0));
            control.DataRefresh += ((sender, args) => {
                GridDataRefresh(control, property, gridItemType);
            });
            control.CreateNewClick += ((sender, args) => {
                GridCreateNewItem(control, property, gridItemType);
            });
            control.EditClick += ((sender, args) => {
                GridItemEdit(control, property, gridItemType);
            });            
            control.RowDoubleClick+= ((sender, args) => {
                GridItemEdit(control, property, gridItemType);
            });
            control.DeleteClick += ((sender, args) => {
                GridItemDelete(control, property, gridItemType);
            });
            control.AllowAdd = control.AllowDelete = LiftToTrue(SecurityHelper.IsEditAllowed(property));
            control.AllowEdit = true;
            control.SelectButtonVisible = false;
            return control;
        }

        bool LiftToTrue(bool? value) {
            return (value == true || value == null);
        }

        protected virtual void GridDataRefresh(GridUserControl grid, PropertyInfo itemsSourceProperty, Type gridItemType) {
            var collection = (itemsSourceProperty.GetValue(BusinessObject) as IEnumerable);
            grid.ItemsSource = collection.ToBindingList(gridItemType);
        }

        protected virtual void GridCreateNewItem(GridUserControl grid, PropertyInfo itemsSourceProperty, Type gridItemType) {
            var collection = (itemsSourceProperty.GetValue(BusinessObject));
            if (collection == null) {
                return;
            }
            DataObjectBase item = UIHelper.CreateObject(gridItemType, this, dbContext, false, (t) => {
                return GridAfterNewItemInitialized(itemsSourceProperty, t);
            });
            if (item != null) {
                if (collection is IList) {
                    IList list = (IList)collection;
                    if (!list.Contains(item)) {
                        list.Add(item);
                    }
                } else {
                    MethodInfo addMethod = itemsSourceProperty.PropertyType.GetMethod("Add", new Type[] { gridItemType });
                    if (addMethod != null) {
                        addMethod.Invoke(collection, new object[] { item });
                    }
                }
                BusinessObject.NotifyPropertyChanged(itemsSourceProperty.Name);
            }
        }

        protected virtual bool GridAfterNewItemInitialized(PropertyInfo collectionProperty, object createdItem) {
            return true;
        }

        protected virtual void GridItemEdit(GridUserControl grid, PropertyInfo itemsSourceProperty, Type gridItemType) {
            if (grid.SelectedItem != null) {
                UIHelper.EditObject((DataObjectBase)grid.SelectedItem, this, dbContext, false);
                BusinessObject.NotifyPropertyChanged(itemsSourceProperty.Name);
            }
        }

        protected virtual void GridItemDelete(GridUserControl grid, PropertyInfo itemsSourceProperty, Type gridItemType) {
            if (grid.SelectedItem != null) {
                var collection = (itemsSourceProperty.GetValue(BusinessObject));
                if (collection == null) {
                    return;
                }
                if (collection is IList) {
                    ((IList)collection).Remove(grid.SelectedItem);
                } else {
                    MethodInfo removeMethod = itemsSourceProperty.PropertyType.GetMethod("Remove", new Type[] { gridItemType });
                    if (removeMethod != null) {
                        removeMethod.Invoke(collection, new object[] { grid.SelectedItem });
                        BusinessObject.NotifyPropertyChanged(itemsSourceProperty.Name);
                    }
                }
            }
        }

        protected virtual Control CreateTabControl() {
            TabControl control = new TabControl();
            control.Margin = new Padding(0, (int)(16 * this.DeviceDpi / 96.0), 0, 0);
            return control;
        }

        protected virtual Control CreateTabItem(PropertyInfo property) {
            var tabItem = new TabPage();
            tabItem.Text = ReflectionHelper.GetPropertyName(property);
            if (ReflectionHelper.IsPropertyRequired(property)) {
                tabItem.Text += "*";
            }
            return tabItem;
        }

        protected Binding SetupBinding(Control control, string controlProperty, PropertyInfo modelProperty) {
            string format = ReflectionHelper.GetPropertyDisplayFormat(modelProperty);
            Binding binding;
            if (string.IsNullOrEmpty(format)) {
                binding = control.DataBindings.Add(controlProperty, BusinessObject, modelProperty.Name, true, DataSourceUpdateMode.OnPropertyChanged);
            } else {
                binding = control.DataBindings.Add(controlProperty, BusinessObject, modelProperty.Name, true, DataSourceUpdateMode.OnPropertyChanged, null, format);
            }            
            return binding;
        }

        void OnBusinessObjectChanged(object sender, PropertyChangedEventArgs e) {
            OnBusinessObjectChanged(e.PropertyName);
        }

        protected virtual void OnBusinessObjectChanged(string propertyName) {
            BusinessObject.OnPropertyChanged(dbContext, propertyName);
        }

        protected virtual List<DataObjectBase> GetCollectionFromDbContext(Type entityType) {
            return EFHelper.GetObjectCollection(dbContext, entityType);
        }
    }
}
