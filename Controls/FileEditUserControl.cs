using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Core.Controls {
    public partial class FileEditUserControl : UserControl, ISupportInitialize {
        public FileEditUserControl() {
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
                        throw new ArgumentException(string.Format("Размер загруженного файла превышает максимально допустимый размер {0} Мб.", MaxBlobSize / 1024.0 / 1024.0));
                    }
                    string fileName = FileNameFromBlob(value);
                    blobData = value;                  
                    txbFileName.Text = fileName;
                    RefreshControlStatus();
                }
            }
        }

        [DefaultValue(null)]
        public string FileName
        {
            get
            {
                return FileNameFromBlob(BlobData);
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

        void RefreshControlStatus() {
            btnLoad.Visible = !ReadOnly;
            btnClear.Enabled = !ReadOnly && BlobData != null && BlobData.Length > 0;
        }

        string FileNameFromBlob(byte[] data) {
            if (data == null || data.Length < 256) {
                return null;
            }
            return System.Text.Encoding.UTF8.GetString(data, 0, 256);
        }

        byte[] CreateBlob(byte[] fileData, string fileName) {
            using (var stream = new MemoryStream())
            {
                byte[] fileNameUtf8 = System.Text.Encoding.UTF8.GetBytes(fileName);
                if (fileNameUtf8.Length <= 256)
                {
                    stream.Write(fileNameUtf8);
                    byte[] zeroBytes = new byte[256 - fileNameUtf8.Length];
                    stream.Write(zeroBytes);
                }
                else
                {
                    stream.Write(fileNameUtf8, 0, 256);
                }
                stream.Write(fileData);
                return stream.ToArray();
            }
        }


        void btnLoad_Click(object sender, EventArgs e) {
            LoadFile();
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

        private void FileEditUserControl_Resize(object sender, EventArgs e) {
        }

        void SaveFile()
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.FileName = FileName;
                dlg.Filter = "Все файлы|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    byte[] fileData = new byte[BlobData.Length - 256];
                    Array.Copy(BlobData, 256, fileData, 0, BlobData.Length - 256);
                    File.WriteAllBytes(dlg.FileName, fileData);
                    var startInfo = new ProcessStartInfo()
                    {
                        FileName = dlg.FileName,
                        WorkingDirectory = Path.GetDirectoryName(dlg.FileName),
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(startInfo);
                }
            }
        }

        void LoadFile()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                UIHelper.Execute(this.ParentForm, () => {
                    byte[] data = File.ReadAllBytes(openFileDialog1.FileName);
                    BlobData = CreateBlob(data, Path.GetFileName(openFileDialog1.FileName));
                });
            }
        }

        private void txbFileName_Click(object sender, EventArgs e)
        {
            if (BlobData != null && BlobData.Length != 0)
            {
                SaveFile();
            }
            else
            {
                LoadFile();
            }
        }
    }
}
