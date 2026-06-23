using ExcelDataReader;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class Form1 : Form
    {
        // ========== 14a. DEKLARASI OBJEK DAL ==========
        DAL dbLogic = new DAL();
        private BindingSource bindingSource;

        public Form1()
        {
            InitializeComponent();
            bindingSource = new BindingSource();

            bindingNavigator1.Visible = true;
            bindingNavigator1.AddNewItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            bindingNavigator1.DeleteItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            bindingNavigator1.MoveFirstItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            bindingNavigator1.MovePreviousItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            bindingNavigator1.MoveNextItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            bindingNavigator1.MoveLastItem.DisplayStyle = ToolStripItemDisplayStyle.Image;

            bindingNavigator1.MoveFirstItem.Text = "<<";
            bindingNavigator1.MovePreviousItem.Text = "<";
            bindingNavigator1.MoveNextItem.Text = ">";
            bindingNavigator1.MoveLastItem.Text = ">>";

            bindingNavigator1.MoveFirstItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
            bindingNavigator1.MovePreviousItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
            bindingNavigator1.MoveNextItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
            bindingNavigator1.MoveLastItem.DisplayStyle = ToolStripItemDisplayStyle.Text;

            bindingNavigator1.AddNewItem.Click += bindingNavigatorAddNewItem_Click;
            bindingNavigator1.DeleteItem.Click += bindingNavigatorDeleteItem_Click;
        }

        // ========== 14b. METHOD SIMPANLOG (MENGGUNAKAN DAL) ==========
        public void SimpanLog(string message)
        {
            dbLogic.InsertLog(message);
        }

        private void LoadComboBoxProdi()
        {
            try
            {
                DataTable dtProdi = dbLogic.GetProdi();
                cmbNamaProdi.Items.Clear();
                foreach (DataRow row in dtProdi.Rows)
                {
                    cmbNamaProdi.Items.Add(row["NamaProdi"].ToString());
                }
                if (cmbNamaProdi.Items.Count > 0)
                    cmbNamaProdi.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Gagal memuat daftar prodi: " + ex.Message);
            }
        }

        // ========== GET KODE PRODI DARI NAMA PRODI ==========
        private string GetKodeProdi(string namaProdi)
        {
            string kode = "";
            try
            {
                using (SqlConnection conn = new SqlConnection(dbLogic.GetConnectionString()))
                {
                    string query = "SELECT KodeProdi FROM ProgramStudi WHERE NamaProdi = @NamaProdi";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NamaProdi", namaProdi);
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                            kode = result.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Error get kode prodi: " + ex.Message);
            }
            return kode;
        }

        // ========== 14c. CONNECT TEST (MENGGUNAKAN DAL) ==========
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbLogic.GetConnectionString()))
                {
                    conn.Open();
                    MessageBox.Show("Koneksi Berhasil", "Sukses",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (SqlException ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ========== 14b. LOAD DATA (MENGGUNAKAN DAL) ==========
        private void LoadData()
        {
            try
            {
                DataTable dt = dbLogic.GetMhs();
                bindingSource.DataSource = dt;
                dataGridView1.DataSource = bindingSource;
                bindingNavigator1.BindingSource = bindingSource;

                // Atur kolom Foto
                if (dataGridView1.Columns["Foto"] != null)
                {
                    DataGridViewImageColumn fotoColumn = (DataGridViewImageColumn)dataGridView1.Columns["Foto"];
                    fotoColumn.ImageLayout = DataGridViewImageCellLayout.Stretch;
                }

                BindControls();
                HitungTotal();

                dataGridView1.Enabled = true;
                btnInsert.Enabled = true;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;
                btnCari.Enabled = true;
                btnLoad.Enabled = true;
                btnResetData.Enabled = true;
                btnTestInjection.Enabled = true;

                lblStatus.Text = $"✅ Berhasil memuat {dt.Rows.Count} data mahasiswa";
                lblStatus.ForeColor = System.Drawing.Color.Green;
            }
            catch (SqlException ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Gagal memuat data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "❌ Gagal memuat data";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Gagal memuat data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "❌ Gagal memuat data";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
        }

        // ========== BIND CONTROLS ==========
        private void BindControls()
        {
            txtNIM.DataBindings.Clear();
            txtNama.DataBindings.Clear();
            cmbJK.DataBindings.Clear();
            dtpTanggalLahir.DataBindings.Clear();
            txtAlamat.DataBindings.Clear();
            cmbNamaProdi.DataBindings.Clear();

            txtNIM.DataBindings.Add("Text", bindingSource, "NIM");
            txtNama.DataBindings.Add("Text", bindingSource, "Nama");
            cmbJK.DataBindings.Add("Text", bindingSource, "JenisKelamin");
            dtpTanggalLahir.DataBindings.Add("Value", bindingSource, "TanggalLahir");
            txtAlamat.DataBindings.Add("Text", bindingSource, "Alamat");
            cmbNamaProdi.DataBindings.Add("Text", bindingSource, "NamaProdi");
        }

        // ========== SETUP DATAGRIDVIEW ==========
        private void SetupDataGridView()
        {
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            if (dataGridView1.Columns["NIM"] != null)
                dataGridView1.Columns["NIM"].HeaderText = "NIM";
            if (dataGridView1.Columns["Nama"] != null)
                dataGridView1.Columns["Nama"].HeaderText = "Nama";
            if (dataGridView1.Columns["JenisKelamin"] != null)
                dataGridView1.Columns["JenisKelamin"].HeaderText = "Jenis Kelamin";
            if (dataGridView1.Columns["TanggalLahir"] != null)
                dataGridView1.Columns["TanggalLahir"].HeaderText = "Tanggal Lahir";
            if (dataGridView1.Columns["Alamat"] != null)
                dataGridView1.Columns["Alamat"].HeaderText = "Alamat";
            if (dataGridView1.Columns["NamaProdi"] != null)
                dataGridView1.Columns["NamaProdi"].HeaderText = "Nama Prodi";
        }

        // ========== TOMBOL LOAD ==========
        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        // ========== 14b. HITUNG TOTAL (MENGGUNAKAN DAL) ==========
        private void HitungTotal()
        {
            try
            {
                int total = dbLogic.CountMhs();
                lblTotal.Text = "Total Mahasiswa: " + total.ToString();
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Gagal menghitung total: " + ex.Message);
            }
        }

        // ========== 14c. INSERT (MENGGUNAKAN DAL) ==========
        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNIM.Text))
            {
                MessageBox.Show("❌ NIM harus diisi!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNIM.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("❌ Nama harus diisi!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNama.Focus();
                return;
            }
            if (cmbJK.SelectedItem == null)
            {
                MessageBox.Show("❌ Jenis Kelamin harus dipilih!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbJK.Focus();
                return;
            }
            if (cmbNamaProdi.SelectedItem == null)
            {
                MessageBox.Show("❌ Nama Prodi harus dipilih!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbNamaProdi.Focus();
                return;
            }

            try
            {
                byte[] imgBytes = null;
                if (pbFoto.Image != null)
                {
                    imgBytes = ImageToByteArray(pbFoto.Image);
                }

                string kodeProdi = GetKodeProdi(cmbNamaProdi.SelectedItem.ToString());

                dbLogic.InsertMhs(
                    txtNIM.Text,
                    txtNama.Text,
                    txtAlamat.Text,
                    cmbJK.Text,
                    dtpTanggalLahir.Value.Date,
                    kodeProdi,
                    imgBytes
                );

                MessageBox.Show("Data mahasiswa berhasil ditambahkan", "Sukses",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                SimpanLog("Rollback Insert : " + ex.Message);
                MessageBox.Show("SQL Error : " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                SimpanLog("General Error : " + ex.Message);
                MessageBox.Show("General Error : " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ========== 14c. UPDATE (MENGGUNAKAN DAL) ==========
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNIM.Text))
            {
                MessageBox.Show("❌ Pilih data yang akan diupdate terlebih dahulu!",
                    "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbNamaProdi.SelectedItem == null)
            {
                MessageBox.Show("❌ Nama Prodi harus dipilih!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbNamaProdi.Focus();
                return;
            }

            DialogResult confirm = MessageBox.Show("Yakin ingin mengubah data mahasiswa ini?",
                "Konfirmasi Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    byte[] imgBytes = null;
                    if (pbFoto.Image != null)
                    {
                        imgBytes = ImageToByteArray(pbFoto.Image);
                    }

                    string kodeProdi = GetKodeProdi(cmbNamaProdi.SelectedItem.ToString());

                    dbLogic.UpdateMhs(
                        txtNIM.Text,
                        txtNama.Text,
                        txtAlamat.Text,
                        cmbJK.Text,
                        dtpTanggalLahir.Value.Date,
                        kodeProdi,
                        imgBytes
                    );

                    MessageBox.Show("Data mahasiswa berhasil diubah", "Sukses",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadData();
                }
                catch (SqlException ex)
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("SQL Error : " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("General Error : " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ========== 14c. DELETE (MENGGUNAKAN DAL) ==========
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNIM.Text))
            {
                MessageBox.Show("❌ Pilih data yang akan dihapus terlebih dahulu!",
                    "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Yakin ingin menghapus data?",
                "Konfirmasi",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    dbLogic.DeleteMhs(txtNIM.Text);
                    MessageBox.Show("Data mahasiswa berhasil dihapus", "Sukses",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadData();
                }
                catch (SqlException ex)
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("SQL Error : " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("General Error : " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ========== 14c. RESET DATA (MENGGUNAKAN DAL) ==========
        private void btnResetData_Click(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show(
                "⚠️ Yakin ingin mereset data?\n\nSemua perubahan akan dikembalikan ke data awal!",
                "Konfirmasi Reset",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    dbLogic.ResetData();
                    MessageBox.Show("Data berhasil direset", "Sukses",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                }
                catch (SqlException ex)
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("SQL Error : " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("General Error : " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ========== 14c. TEST INJECTION (MENGGUNAKAN DAL) ==========
        private void btnTestInjection_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNIM.Text))
            {
                MessageBox.Show("Masukkan NIM untuk uji injection!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                dbLogic.TestInject(txtNIM.Text);
                LoadData();
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("safe"))
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("SQL Error : Unsafe UPDATE operation not allowed", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("SQL Error : " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error : " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ========== 14d. CELL CLICK (MENGGUNAKAN DAL) ==========
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                txtNIM.Text = row.Cells["NIM"].Value.ToString();
                txtNama.Text = row.Cells["Nama"].Value.ToString();
                cmbJK.Text = row.Cells["JenisKelamin"].Value.ToString();
                dtpTanggalLahir.Value = Convert.ToDateTime(row.Cells["TanggalLahir"].Value);
                txtAlamat.Text = row.Cells["Alamat"].Value.ToString();

                string prodiValue = row.Cells["NamaProdi"].Value.ToString();
                if (cmbNamaProdi.Items.Contains(prodiValue))
                {
                    cmbNamaProdi.SelectedItem = prodiValue;
                }
                else
                {
                    cmbNamaProdi.Text = prodiValue;
                }

                if (row.Cells["Foto"].Value != DBNull.Value)
                {
                    byte[] foto = (byte[])row.Cells["Foto"].Value;
                    pbFoto.Image = ByteArrayToImage(foto);
                    pbFoto.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                else
                {
                    pbFoto.Image = null;
                }

                txtNIM.Enabled = false;
                lblStatus.Text = $"📌 Terpilih: {txtNama.Text} ({txtNIM.Text}) - Prodi: {prodiValue}";
                lblStatus.ForeColor = System.Drawing.Color.Blue;
            }
        }

        // ========== 14b. CLEAR FORM (MENGGUNAKAN DAL) ==========
        private void ClearForm()
        {
            txtNIM.Enabled = true;
            txtNIM.Clear();
            txtNama.Clear();
            cmbJK.SelectedIndex = -1;
            dtpTanggalLahir.Value = DateTime.Now;
            txtAlamat.Clear();
            cmbNamaProdi.SelectedIndex = -1;
            pbFoto.Image = null;
            txtNIM.Focus();
        }

        // ========== REFRESH ==========
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearForm();
            LoadData();
            lblStatus.Text = "🔄 Data berhasil direfresh";
            lblStatus.ForeColor = System.Drawing.Color.Green;
        }

        // ========== EXIT ==========
        private void btnExit_Click(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show("Yakin ingin keluar dari aplikasi?",
                "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        // ========== TOMBOL DASHBOARD ==========
        private void btnDashboard_Click(object sender, EventArgs e)
        {
            FormDashboard frmDashboard = new FormDashboard();
            frmDashboard.Show();
            this.Hide();
        }

        // ========== TOMBOL REKAP DATA ==========
        private void btnRekapData_Click(object sender, EventArgs e)
        {
            Form3 frmRekap = new Form3();
            frmRekap.Show();
            this.Hide();
        }

        // ========== TOMBOL CARI ==========
        private void btnCari_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNIM.Text))
            {
                MessageBox.Show("Masukkan NIM yang akan dicari!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNIM.Focus();
                return;
            }

            try
            {
                DataTable dt = dbLogic.GetMhsByNIM(txtNIM.Text);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    txtNama.Text = row["Nama"].ToString();
                    cmbJK.Text = row["JenisKelamin"].ToString();
                    dtpTanggalLahir.Value = Convert.ToDateTime(row["TanggalLahir"]);
                    txtAlamat.Text = row["Alamat"].ToString();
                    cmbNamaProdi.Text = row["NamaProdi"].ToString();

                    if (row["Foto"] != DBNull.Value)
                    {
                        byte[] foto = (byte[])row["Foto"];
                        pbFoto.Image = ByteArrayToImage(foto);
                    }
                    else
                    {
                        pbFoto.Image = null;
                    }

                    lblStatus.Text = $"🔍 Data ditemukan: {txtNama.Text}";
                    lblStatus.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    MessageBox.Show("Data tidak ditemukan!", "Informasi",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ========== TOMBOL UPLOAD GAMBAR ==========
        private void btnUploadGambar_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pbFoto.Image = Image.FromFile(openFileDialog.FileName);
                    pbFoto.SizeMode = PictureBoxSizeMode.StretchImage;
                    lblStatus.Text = "📷 Gambar berhasil diupload!";
                    lblStatus.ForeColor = System.Drawing.Color.Green;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal upload gambar: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ========== TOMBOL IMPORT EXCEL ==========
        private void btnImportExcel_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xlsx;*.xls";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var stream = File.Open(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet();
                            DataTable dtExcel = result.Tables[0];

                            int count = 0;
                            using (SqlConnection conn = new SqlConnection(dbLogic.GetConnectionString()))
                            {
                                conn.Open();
                                foreach (DataRow row in dtExcel.Rows)
                                {
                                    try
                                    {
                                        string nim = row[0].ToString();
                                        string nama = row[1].ToString();
                                        string jk = row[2].ToString();
                                        DateTime tglLahir = Convert.ToDateTime(row[3]);
                                        string alamat = row[4].ToString();
                                        string namaProdi = row[5].ToString();
                                        string kodeProdi = GetKodeProdi(namaProdi);

                                        if (string.IsNullOrEmpty(kodeProdi))
                                            continue;

                                        string query = @"
                                            INSERT INTO Mahasiswa (NIM, Nama, JenisKelamin, Tanggallahir, Alamat, KodeProdi, TanggallDaftar)
                                            VALUES (@NIM, @Nama, @JK, @TglLahir, @Alamat, @KodeProdi, GETDATE())";

                                        using (SqlCommand cmd = new SqlCommand(query, conn))
                                        {
                                            cmd.Parameters.AddWithValue("@NIM", nim);
                                            cmd.Parameters.AddWithValue("@Nama", nama);
                                            cmd.Parameters.AddWithValue("@JK", jk);
                                            cmd.Parameters.AddWithValue("@TglLahir", tglLahir);
                                            cmd.Parameters.AddWithValue("@Alamat", alamat);
                                            cmd.Parameters.AddWithValue("@KodeProdi", kodeProdi);
                                            cmd.ExecuteNonQuery();
                                            count++;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        SimpanLog("Import Excel Error: " + ex.Message);
                                    }
                                }
                            }
                            MessageBox.Show($"✅ Berhasil import {count} data dari Excel!", "Sukses",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("Gagal import Excel: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ========== HELPER: IMAGE TO BYTE ARRAY ==========
        private byte[] ImageToByteArray(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        // ========== HELPER: BYTE ARRAY TO IMAGE ==========
        private Image ByteArrayToImage(byte[] byteArray)
        {
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                return Image.FromStream(ms);
            }
        }

        // ========== BINDING NAVIGATOR ==========
        private void bindingNavigatorAddNewItem_Click(object sender, EventArgs e)
        {
            ClearForm();
            txtNIM.Focus();
            lblStatus.Text = "📝 Mode: Menambah data baru";
            lblStatus.ForeColor = System.Drawing.Color.Blue;
        }

        private void bindingNavigatorDeleteItem_Click(object sender, EventArgs e)
        {
            if (bindingSource.Current != null)
            {
                DialogResult confirm = MessageBox.Show(
                    "⚠️ Yakin ingin menghapus data yang dipilih?\n\nData yang dihapus tidak dapat dikembalikan!",
                    "Konfirmasi Hapus",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    btnDelete_Click(sender, e);
                }
            }
        }

        // ========== FORM LOAD ==========
        private void Form1_Load(object sender, EventArgs e)
        {
            cmbJK.DataSource = new string[] { "L", "P" };
            cmbJK.DropDownStyle = ComboBoxStyle.DropDownList;

            cmbNamaProdi.DropDownStyle = ComboBoxStyle.DropDownList;
            LoadComboBoxProdi();

            SetupDataGridView();
            bindingNavigator1.BindingSource = bindingSource;

            LoadData();

            lblStatus.Text = "✅ Aplikasi siap digunakan.";
            lblStatus.ForeColor = System.Drawing.Color.Green;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Optional: kosong
        }
    }
}