using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Reflection;

namespace Core.Controls {
    public partial class GridUserControl : UserControl {
        public GridUserControl() {
            InitializeComponent();
            Grid.AutoGenerateColumns = false;
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
        }

        public event EventHandler DataRefresh;
        public event EventHandler CreateNewClick;
        public event EventHandler EditClick;
        public event EventHandler RowDoubleClick;
        public event EventHandler DeleteClick;
        public event EventHandler SelectClick;

        bool allowAdd = true;
        bool allowDelete = true;
        bool allowEdit = true;
        bool allowFind = true;
        bool selectButtonVisible = false;
        bool toolbarVisibility = true;

        public bool AllowAdd {
            get { return allowAdd; }
            set { btnCreateNew.Visible = allowAdd = value; }
        }

        public bool AllowDelete {
            get { return allowDelete; }
            set { btnDelete.Visible = allowDelete = value; }
        }

        public bool AllowEdit {
            get { return allowEdit; }
            set { btnEdit.Visible = allowEdit = value; }
        }

        public bool AllowFind {
            get { return allowFind; }
            set { txbFind.Visible = lblFind.Visible = toolStripSeparatorBeforeFind.Visible = allowFind = value; }
        }

        [DefaultValue(false)]
        public bool SelectButtonVisible {
            get { return selectButtonVisible; }
            set { btnSelect.Visible = btnSelectSeparator.Visible = selectButtonVisible = value; }
        }

        [DefaultValue(true)]
        public bool ToolbarVisibility {
            get { return toolbarVisibility; }
            set { Toolbar.Visible = toolbarVisibility = value; }
        }

        public String AddButtonText { get { return btnCreateNew.Text; } set { btnCreateNew.Text = value; } }

        public String EditButtonText { get { return btnEdit.Text; } set { btnEdit.Text = value; } }

        public String DeleteButtonText { get { return btnDelete.Text; } set { btnDelete.Text = value; } }

        public String FindText { get { return txbFind.Text; } set { txbFind.Text = value; } }

        public String FindLabelText { get { return lblFind.Text; } set { lblFind.Text = value; } }

        [Browsable(false)]
        public object SelectedItem {
            get {
                return Grid.SelectedItem;
            }
            set {
                Grid.SelectedItem = value;
            }
        }

        public GridControl Grid {
            get {
                return dataGrid;
            }
        }

        public IList ItemsSource { 
            get {
                return Grid.ItemsSource;
            }
            set {
                Grid.ItemsSource = value;
            }
        }

        public string SortProperty {
            get {
                return Grid.SortProperty;
            }
            set {
                Grid.SortProperty = value;
            }
        }

        public ListSortDirection SortDirection {
            get {
                return Grid.SortDirection;
            }
            set {
                Grid.SortDirection = value;
            }
        }

        public void SetupSecurity(Type itemType) {
            bool itemAllowEdit = LiftToTrue(SecurityHelper.IsCreateAllowed(itemType));
            AllowAdd = LiftToTrue(SecurityHelper.IsCreateAllowed(itemType));
            AllowEdit = true;
            AllowDelete = LiftToTrue(SecurityHelper.IsDeleteAllowed(itemType));
            EditButtonText = (itemAllowEdit) ? "Редактировать" : "Открыть";
            ToolbarVisibility = AllowEdit || AllowFind || allowDelete || AllowAdd;
        }

        bool LiftToTrue(bool? value) {
            return (value == true || value == null);
        }

        public void AutoCreateColumns(Type itemType) {
            Grid.AutoCreateColumns(itemType);
        }

        public void ApplyDefaultSort(Type itemType) {
            Grid.ApplyDefaultSort(itemType);
        }

        private void txbFind_TextChanged(object sender, EventArgs e) {
            FindText = txbFind.Text;
            if (DataRefresh != null) {
                try {
                    Cursor.Current = Cursors.WaitCursor;
                    DataRefresh(this, e);
                } finally {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void dataGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            if (e.RowIndex < 0) {
                return;
            }
            if (RowDoubleClick != null && AllowEdit) {
                if (SelectedItem != null) {
                    try {
                        Cursor.Current = Cursors.WaitCursor;
                        RowDoubleClick(this, e);
                        if (DataRefresh != null) {
                            Cursor.Current = Cursors.WaitCursor;
                            DataRefresh(this, e);
                        }
                    } finally {
                        Cursor.Current = Cursors.Default;
                    }
                }
            }
        }

        private void GridControl_Load(object sender, EventArgs e) {
            if (DataRefresh != null) {
                DataRefresh(this, e);
            }
        }

        private void btnCreateNew_Click(object sender, EventArgs e) {
            if (CreateNewClick != null) {
                try {
                    Cursor.Current = Cursors.WaitCursor;
                    CreateNewClick(this, e);
                    if (DataRefresh != null) {
                        DataRefresh(this, e);
                    }
                } finally {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e) {
            if (EditClick != null) {
                if (SelectedItem != null) {
                    try {
                        Cursor.Current = Cursors.WaitCursor;
                        EditClick(this, e);
                        if (DataRefresh != null) {
                            Cursor.Current = Cursors.WaitCursor;
                            DataRefresh(this, e);
                        }
                    } finally {
                        Cursor.Current = Cursors.Default;
                    }
                } else {
                    MessageBox.Show("Необходимо выбрать элемент для редактирования.", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e) {
            if (DeleteClick != null) {
                if (SelectedItem != null) {
                    try {
                        Cursor.Current = Cursors.WaitCursor;
                        DeleteClick(this, e);
                        if (DataRefresh != null) {
                            Cursor.Current = Cursors.WaitCursor;
                            DataRefresh(this, e);
                        }
                    } finally {
                        Cursor.Current = Cursors.Default;
                    }
                } else {
                    MessageBox.Show("Необходимо выбрать элемент для удаления.", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnSelect_Click(object sender, EventArgs e) {
            if (SelectClick != null) {
                SelectClick(this, e);
            }
        }

        private void dataGrid_DataError(object sender, DataGridViewDataErrorEventArgs e) {
            e.ThrowException = false;
            e.Cancel = false;
        }

        private void dataGrid_SelectionChanged(object sender, EventArgs e) {
            btnEdit.Enabled = btnDelete.Enabled = btnSelect.Enabled = (Grid.SelectedRows.Count > 0);
        }
    }
}
