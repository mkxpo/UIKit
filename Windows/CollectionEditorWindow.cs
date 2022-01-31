using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Core.Windows {
    public partial class CollectionEditorWindow : Form {

        protected DbContext dbContext;
        protected Type itemType;
        protected bool isSelectionRequired;

        public CollectionEditorWindow() {
            InitializeComponent();
        }

        public CollectionEditorWindow(Type itemType, DbContext dbContext, bool isSelectionRequired = false) {
            InitializeComponent();
            this.dbContext = dbContext;
            this.itemType = itemType;
            this.isSelectionRequired = isSelectionRequired;
            Grid.SelectButtonVisible = isSelectionRequired;
            Grid.AutoCreateColumns(itemType);
            Grid.SetupSecurity(itemType);            
            Grid.DataRefresh += ((s, args) => {
                GridDataRefresh();
            });
            Grid.CreateNewClick += OnGridCreateNewClick;
            Grid.EditClick += OnGridEditClick;
            Grid.RowDoubleClick += OnGridRowDoubleClick;
            Grid.DeleteClick += OnGridDeleteClick;
            Grid.SelectClick += OnGridSelectClick;
            this.Text = GetWindowTitle();
            SetupSecurity();
        }

        private void OnGridSelectClick(object sender, EventArgs e) {
            if (Grid.SelectedItem != null) {
                SelectedItem = Grid.SelectedItem;
                DialogResult = DialogResult.OK;
            } else {
                UIHelper.Warning(this, "Необходимо выбрать элемент из списка.");
            }
        }

        public object SelectedItem { get; private set; }

        protected virtual void OnGridCreateNewClick(object sender, EventArgs e) {
            UIHelper.CreateObject(itemType, this, dbContext, true);
        }

        protected virtual void OnGridEditClick(object sender, EventArgs e) {
            if (Grid.SelectedItem != null) {
                UIHelper.EditObject((DataObjectBase)Grid.SelectedItem, this, dbContext, true);
            }
        }

        protected virtual void OnGridRowDoubleClick(object sender, EventArgs e) {
            if (Grid.SelectedItem != null) {
                if (isSelectionRequired) {
                    SelectedItem = Grid.SelectedItem;
                    DialogResult = DialogResult.OK;
                } else {
                    UIHelper.EditObject((DataObjectBase)Grid.SelectedItem, this, dbContext, true);
                }
            }            
        }

        protected virtual void OnGridDeleteClick(object sender, EventArgs e) {
            if (Grid.SelectedItem != null) {
                UIHelper.DeleteObject((DataObjectBase)Grid.SelectedItem, dbContext, true);
            }
        }

        private void CollectionEditorWindow_Load(object sender, EventArgs e) {
            if (itemType == null) {
                return;
            }
            Grid.ApplyDefaultSort(itemType);
        }

        protected virtual void SetupSecurity() {
            if (SecurityHelper.IsReadAllowed(itemType) == false) {
                throw new UnauthorizedAccessException(string.Format("Недостаточно прав доступа для просмотра списка объектов '{0}'.", ReflectionHelper.GetTypeName(itemType)));
            }
        }

        protected virtual string GetWindowTitle() {
            if (isSelectionRequired) {
                return string.Format("{0} - выбор из списка", ReflectionHelper.GetTypeName(itemType));
            } else {
                return string.Format("{0} - список", ReflectionHelper.GetTypeName(itemType));
            }
        }

        protected virtual void GridDataRefresh() {
            var items = EFHelper.GetObjectCollection(dbContext, itemType);
            if (!string.IsNullOrEmpty(Grid.FindText)) {
                items = items.Where(t => t.ToString().ToLower().Contains(Grid.FindText.ToLower())).ToList();
            }
            Grid.ItemsSource = items.ToBindingList(itemType);
        }
    }
}
