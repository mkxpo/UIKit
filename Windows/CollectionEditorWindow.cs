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

        public CollectionEditorWindow() {
            InitializeComponent();
        }

        CollectionEditorControl collectionEditorControl;

        public CollectionEditorWindow(Type itemType, DbContext dbContext, bool isSelectionRequired = false) {
            InitializeComponent();
            Text = GetWindowTitle(itemType, isSelectionRequired);
            collectionEditorControl = new CollectionEditorControl(itemType, dbContext, isSelectionRequired);
            Controls.Add(collectionEditorControl);
            collectionEditorControl.Dock = DockStyle.Fill;
            collectionEditorControl.Visible = true;
            collectionEditorControl.SelectionChanged += (s, e) => {
                this.DialogResult = DialogResult.OK;
            };
        }

        protected virtual string GetWindowTitle(Type itemType, bool isSelectionRequired) {
            if (isSelectionRequired) {
                return string.Format("{0} - выбор из списка", ReflectionHelper.GetTypeName(itemType));
            } else {
                return string.Format("{0} - список", ReflectionHelper.GetTypeName(itemType));
            }
        }

        public object SelectedItem {
            get {
                return collectionEditorControl?.SelectedItem;
            }
        }
    }
}
