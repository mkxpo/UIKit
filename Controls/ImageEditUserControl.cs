using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Core.Controls {
    public partial class ImageEditUserControl : UserControl, ISupportInitialize {
        public ImageEditUserControl() {
            BeginInit();
            InitializeComponent();
            EndInit();
        }

        byte[] blobData = null;

        [DefaultValue(null)]
        [Bindable(true)]
        public byte[] BlobData {
            get {
                return blobData;
            }
            set {
                if (blobData != value) {
                    if (value != null && MaxBlobSize > 0 && value.Length > MaxBlobSize && !isInitializing) {
                        throw new ArgumentException(string.Format("Размер загруженного изображения превышает максимально допустимый размер {0} Мб.", MaxBlobSize / 1024.0 / 1024.0));
                    }
                    Image newImage = ImageFromBlob(value);
                    blobData = value;
                    DestroyOldImage();                    
                    pictureBox1.Image = newImage;
                    RefreshControlStatus();
                }
            }
        }

        bool readOnly = false;

        [DefaultValue(false)]
        public bool ReadOnly {
            get {
                return readOnly;
            }
            set {
                readOnly = value;
                RefreshControlStatus();
            }
        }

        int maxSize = 0;

        [DefaultValue(0)]
        public int MaxBlobSize {
            get {
                return maxSize;
            }
            set {
                maxSize = value;
            }
        }

        void DestroyOldImage() {
            IDisposable img = pictureBox1.Image as IDisposable;
            if (img != null) {
                img.Dispose();
            }
            pictureBox1.Image = null;
        }

        void RefreshControlStatus() {
            btnLoad.Visible = !ReadOnly;
            btnClear.Visible = !ReadOnly && BlobData != null && BlobData.Length > 0;
            pictureBox1.Enabled = pictureBox1.Image != null;
        }

        Image ImageFromBlob(byte[] data) {
            if (data == null || data.Length == 0) {
                return null;
            }
            using (MemoryStream stream = new MemoryStream(data)) {
                return Image.FromStream(stream);
            }
        }

        byte[] ImageToBlob(Image img) {
            using (var stream = new MemoryStream()) {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        void pictureBox1_Click(object sender, EventArgs e) {
            if (pictureBox1.Image != null) {
                using (Form form = new Form()) {
                    form.Width = 600;
                    form.Height = 400;
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.Text = "Просмотр изображения";
                    PictureBox picture = new PictureBox();
                    form.Controls.Add(picture);
                    picture.Dock = DockStyle.Fill;
                    picture.SizeMode = PictureBoxSizeMode.Zoom;
                    picture.BackColor = Color.Black;
                    picture.Image = pictureBox1.Image;
                    picture.BorderStyle = BorderStyle.FixedSingle;
                    form.ShowDialog();
                }
            }
        }

        void btnLoad_Click(object sender, EventArgs e) {
            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                UIHelper.Execute(this.ParentForm, () => {
                    var image = Image.FromFile(openFileDialog1.FileName);
                    BlobData = ImageToBlob(image);
                });
            }
        }

        bool isInitializing = false;
        public void BeginInit() {
            isInitializing = true;
        }

        public void EndInit() {
            isInitializing = false;
        }

        void btnClear_Click(object sender, EventArgs e) {
            BlobData = null;
        }

        private void ImageEditUserControl_Resize(object sender, EventArgs e) {
            if(pictureBox1.Height + 4 + btnLoad.Width > Width) {
                btnLoad.Left = btnClear.Left = Width - btnLoad.Left;
                pictureBox1.Width = btnLoad.Left - 4;
            } else {
                pictureBox1.Width = pictureBox1.Height;
                btnLoad.Left = btnClear.Left = pictureBox1.Width + 4;
            }
        }
    }
}
