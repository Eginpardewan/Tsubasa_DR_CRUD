using ExcelDataReader;
using System;
using System.Data;
using ExcelDataReader;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class Form1 : Form
    {
        // ========== KONEKSI DATABASE ==========
        private readonly string connectionString =
            "Data Source=LAPTOP-SDC5DOB7\\EGIN;Initial Catalog=DBakademikADO;Integrated Security=True";

        // DataTable untuk menyimpan data
        private DataTable dtMahasiswa;
        private BindingSource bindingSource;

        public Form1()
        {
            InitializeComponent();
            bindingSource = new BindingSource();

            // ========== SET BINDING NAVIGATOR AGAR TOMBOL KELIHATAN ==========
            bindingNavigator1.Visible = true;

            // Set tombol navigasi menggunakan Image (bukan Text)
            bindingNavigator1.AddNewItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            bindingNavigator1.DeleteItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            bindingNavigator1.MoveFirstItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            bindingNavigator1.MovePreviousItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            bindingNavigator1.MoveNextItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
            bindingNavigator1.MoveLastItem.DisplayStyle = ToolStripItemDisplayStyle.Image;

            // Set teks untuk tombol navigasi
            bindingNavigator1.MoveFirstItem.Text = "<<";
            bindingNavigator1.MovePreviousItem.Text = "<";
            bindingNavigator1.MoveNextItem.Text = ">";
            bindingNavigator1.MoveLastItem.Text = ">>";

            // Set agar tombol menampilkan teks (karena gambar tidak tersedia)
            bindingNavigator1.MoveFirstItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
            bindingNavigator1.MovePreviousItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
            bindingNavigator1.MoveNextItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
            bindingNavigator1.MoveLastItem.DisplayStyle = ToolStripItemDisplayStyle.Text;

            // Menambahkan event handler untuk tombol navigasi
            bindingNavigator1.AddNewItem.Click += bindingNavigatorAddNewItem_Click;
            bindingNavigator1.DeleteItem.Click += bindingNavigatorDeleteItem_Click;
        }

        // ========== PRAKTIKUM 2: METHOD LOGGING ==========
        private void SimpanLog(string pesan)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO LogError (waktu, pesan_error) VALUES (GETDATE(), @pesan)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@pesan", pesan);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan log: " + ex.Message);
            }
        }

        // ========== LOAD COMBOBOX NAMA PRODI ==========
        private void LoadComboBoxProdi()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT KodeProdi, NamaProdi FROM ProgramStudi ORDER BY NamaProdi";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        cmbNamaProdi.Items.Clear();
                        while (reader.Read())
                        {
                            string nama = reader["NamaProdi"].ToString();
                            cmbNamaProdi.Items.Add(nama);
                        }
                        if (cmbNamaProdi.Items.Count > 0)
                            cmbNamaProdi.SelectedIndex = 0;
                    }
                }
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
                using (SqlConnection conn = new SqlConnection(connectionString))
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

        // ========== CONNECT TEST ==========
        private void ConnectDatabase()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    MessageBox.Show("✅ Koneksi ke database BERHASIL!", "Sukses",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("❌ Koneksi ke database GAGAL!\n\n" +
                    "Pastikan:\n" +
                    "1. SQL Server sedang berjalan\n" +
                    "2. Database 'DBakademikADO' sudah dibuat\n\n" +
                    "Error Detail: " + ex.Message,
                    "Error Koneksi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            ConnectDatabase();
        }

        // ========== LOAD DATA DARI STORED PROCEDURE ==========
        private void LoadData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_GetMahasiswa", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            dtMahasiswa = new DataTable();
                            da.Fill(dtMahasiswa);

                            bindingSource.DataSource = dtMahasiswa;
                            dataGridView1.DataSource = bindingSource;
                            bindingNavigator1.BindingSource = bindingSource;

                            BindControls();

                            lblStatus.Text = $"✅ Berhasil memuat {dtMahasiswa.Rows.Count} data mahasiswa";
                            lblStatus.ForeColor = System.Drawing.Color.Green;
                        }
                    }
                }
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

            HitungTotal();
        }

        // ========== BIND CONTROLS ==========
        private void BindControls()
        {
            // Clear existing bindings
            txtNIM.DataBindings.Clear();
            txtNama.DataBindings.Clear();
            cmbJK.DataBindings.Clear();
            dtpTanggalLahir.DataBindings.Clear();
            txtAlamat.DataBindings.Clear();
            cmbNamaProdi.DataBindings.Clear();

            // Add new bindings ke BindingSource
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

            // Rename header kolom
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

        // ========== INSERT ==========
        private void btnInsert_Click(object sender, EventArgs e)
        {
            // Validasi input
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

            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                // Dapatkan KodeProdi dari NamaProdi yang dipilih
                string kodeProdi = GetKodeProdi(cmbNamaProdi.SelectedItem.ToString());

                // 1. Insert ke Mahasiswa via Stored Procedure
                SqlCommand cmd = new SqlCommand("sp_InsertMahasiswa", conn, trans);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@pNIM", txtNIM.Text);
                cmd.Parameters.AddWithValue("@pNama", txtNama.Text);
                cmd.Parameters.AddWithValue("@pAlamat", txtAlamat.Text);
                cmd.Parameters.AddWithValue("@pJenisKelamin", cmbJK.Text);
                cmd.Parameters.AddWithValue("@pTanggalLahir", dtpTanggalLahir.Value.Date);
                cmd.Parameters.AddWithValue("@pKodeProdi", kodeProdi);
                cmd.Parameters.AddWithValue("@pFoto", pbFoto.Image != null ? ImageToByteArray(pbFoto.Image) : (object)DBNull.Value);
                cmd.ExecuteNonQuery();

                // 2. Insert ke LogAktivitasSalah (logging)
                SqlCommand cmdLog = new SqlCommand(
                    "INSERT INTO LogAktivitasSalah (aktivitas, waktu) VALUES (@aktivitas, GETDATE())",
                    conn, trans);
                cmdLog.Parameters.AddWithValue("@aktivitas", "INSERT MAHASISWA : " + txtNIM.Text);
                cmdLog.ExecuteNonQuery();

                trans.Commit();
                MessageBox.Show("Data berhasil ditambahkan", "Sukses",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                trans.Rollback();
                SimpanLog("ROLLBACK INSERT : " + ex.Message);
                MessageBox.Show("Error database: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                trans.Rollback();
                SimpanLog("GENERAL ERROR : " + ex.Message);
                MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }

        // ========== UPDATE ==========
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
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string kodeProdi = GetKodeProdi(cmbNamaProdi.SelectedItem.ToString());
                        string query = @"
                            UPDATE Mahasiswa 
                            SET Nama = @Nama,
                                JenisKelamin = @JenisKelamin,
                                Tanggallahir = @TanggalLahir,
                                Alamat = @Alamat,
                                KodeProdi = @KodeProdi,
                                Foto = @Foto
                            WHERE NIM = @NIM";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@NIM", txtNIM.Text.Trim());
                            cmd.Parameters.AddWithValue("@Nama", txtNama.Text.Trim());
                            cmd.Parameters.AddWithValue("@JenisKelamin", cmbJK.Text);
                            cmd.Parameters.AddWithValue("@TanggalLahir", dtpTanggalLahir.Value.Date);
                            cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text.Trim());
                            cmd.Parameters.AddWithValue("@KodeProdi", kodeProdi);
                            cmd.Parameters.AddWithValue("@Foto", pbFoto.Image != null ? ImageToByteArray(pbFoto.Image) : (object)DBNull.Value);

                            conn.Open();
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show($"✅ Data berhasil diupdate! ({rowsAffected} baris)", "Sukses",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearForm();
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("❌ Data tidak ditemukan! Periksa NIM.", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("Error database: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("Error: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ========== DELETE ==========
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNIM.Text))
            {
                MessageBox.Show("❌ Pilih data yang akan dihapus terlebih dahulu!",
                    "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"⚠️ Yakin ingin menghapus data mahasiswa dengan NIM {txtNIM.Text}?",
                "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_DeleteMahasiswa", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@pNIM", txtNIM.Text.Trim());

                            conn.Open();
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("✅ Data berhasil dihapus!", "Sukses",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("⚠️ Data tidak ditemukan!", "Informasi",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            ClearForm();
                            LoadData();
                        }
                    }
                }
                catch (SqlException ex)
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("Error database: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("Error: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ========== HITUNG TOTAL ==========
        private void HitungTotal()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_CountMahasiswa", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter outputParam = new SqlParameter("@Total", SqlDbType.Int);
                        outputParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outputParam);

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        lblTotal.Text = "Total Mahasiswa: " + outputParam.Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Gagal menghitung total: " + ex.Message);
            }
        }

        // ========== CELL CLICK ==========
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

                // Tampilkan foto jika ada
                if (row.Cells["Foto"].Value != DBNull.Value)
                {
                    byte[] foto = (byte[])row.Cells["Foto"].Value;
                    pbFoto.Image = ByteArrayToImage(foto);
                }
                else
                {
                    pbFoto.Image = null;
                }

                lblStatus.Text = $"📌 Terpilih: {txtNama.Text} ({txtNIM.Text}) - Prodi: {prodiValue}";
                lblStatus.ForeColor = System.Drawing.Color.Blue;
            }
        }

        // ========== CLEAR FORM ==========
        private void ClearForm()
        {
            txtNIM.Clear();
            txtNama.Clear();
            cmbJK.SelectedIndex = -1;
            dtpTanggalLahir.Value = DateTime.Now;
            txtAlamat.Clear();
            pbFoto.Image = null;
            if (cmbNamaProdi.Items.Count > 0)
                cmbNamaProdi.SelectedIndex = 0;
            else
                cmbNamaProdi.SelectedIndex = -1;
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

        // ========== RESET DATA ==========
        private void btnResetData_Click(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show(
                "⚠️ Yakin ingin mereset data?\n\nSemua perubahan akan dikembalikan ke data awal!",
                "Konfirmasi Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        string query = @"
                            IF OBJECT_ID('dbo.Mahasiswa_Backup') IS NOT NULL
                            BEGIN
                                DELETE FROM dbo.Mahasiswa;
                                INSERT INTO dbo.Mahasiswa 
                                SELECT * FROM dbo.Mahasiswa_Backup;
                            END";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("✅ Data berhasil direset!", "Sukses",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadData();
                    }
                }
                catch (Exception ex)
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("❌ Reset gagal: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ========== SQL INJECTION TEST ==========
        private void btnTestInjection_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Mahasiswa SET Nama='" + txtNama.Text + "' WHERE NIM='" + txtNIM.Text + "'";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    MessageBox.Show($"Update berhasil! {rowsAffected} baris terupdate.",
                        "Hasil Injection", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    btnDelete_Click(sender, e);
                }
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
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_GetMahasiswaByNIM", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@pNIM", txtNIM.Text.Trim());

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

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
                }
            }
            catch (Exception ex)
            {
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
                    // Import Excel menggunakan ExcelDataReader
                    // Pastikan sudah install NuGet: ExcelDataReader, ExcelDataReader.DataSet
                    using (var stream = File.Open(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet();
                            DataTable dtExcel = result.Tables[0];

                            int count = 0;
                            using (SqlConnection conn = new SqlConnection(connectionString))
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
                                        {
                                            continue;
                                        }

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