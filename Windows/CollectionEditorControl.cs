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
    public partial class CollectionEditorControl : UserControl {

        protected DbContext dbContext;
        protected Type itemType;
        protected bool isSelectionRequired;

        public event EventHandler SelectionChanged;

        public CollectionEditorControl() {
            InitializeComponent();
        }

        public CollectionEditorControl(Type itemType, DbContext dbContext, bool isSelectionRequired = false) {
            InitializeComponent();
            this.dbContext = dbContext;
            this.itemType = itemType;
            this.isSelectionRequired = isSelectionRequired;
            Grid.Visible = true;
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
            SetupSecurity();
        }

        private void OnGridSelectClick(object sender, EventArgs e) {
            if (Grid.SelectedItem != null) {
                SelectedItem = Grid.SelectedItem;
                if (SelectionChanged != null) {
                    SelectionChanged(this, EventArgs.Empty);
                }
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
                    if (SelectionChanged != null) {
                        SelectionChanged(this, EventArgs.Empty);
                    }
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

        protected virtual void GridDataRefresh() {
            var items = EFHelper.GetObjectCollection(dbContext, itemType);
            if (!string.IsNullOrEmpty(Grid.FindText)) {
                items = items.Where(t => GetStringRepresentationForSearch(t).ToLower().Contains(Grid.FindText.ToLower())).ToList();
            }
            Grid.ItemsSource = items.ToBindingList(itemType);
        }

        string GetStringRepresentationForSearch(object obj) {
            if (obj is DataObjectBase dataObject) {
                return dataObject.StringRepresentation;
            } else if (obj != null) {
                return obj.ToString();
            } else {
                return "";
            }
        }
    }
}
