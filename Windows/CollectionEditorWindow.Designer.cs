namespace Core.Windows {
    partial class CollectionEditorWindow {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.Grid = new Core.Controls.GridUserControl();
            this.SuspendLayout();
            // 
            // Grid
            // 
            this.Grid.AddButtonText = "Добавить...";
            this.Grid.AllowAdd = true;
            this.Grid.AllowDelete = true;
            this.Grid.AllowEdit = true;
            this.Grid.AllowFind = true;
            this.Grid.DeleteButtonText = "Удалить";
            this.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Grid.EditButtonText = "Редактировать";
            this.Grid.FindLabelText = "Поиск:";
            this.Grid.FindText = "";
            this.Grid.ItemsSource = null;
            this.Grid.Location = new System.Drawing.Point(0, 0);
            this.Grid.Name = "Grid";
            this.Grid.SelectedItem = null;
            this.Grid.Size = new System.Drawing.Size(800, 450);
            this.Grid.SortDirection = System.ComponentModel.ListSortDirection.Ascending;
            this.Grid.SortProperty = null;
            this.Grid.TabIndex = 0;
            // 
            // CollectionEditorWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.Grid);
            this.Name = "CollectionEditorWindow";
            this.Text = "CollectionEditorWindow";
            this.Load += new System.EventHandler(this.CollectionEditorWindow_Load);
            this.ResumeLayout(false);

        }

        #endregion

        protected Controls.GridUserControl Grid;
    }
}