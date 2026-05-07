using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class Form1 : Form
    {
        // ========== KONEKSI DATABASE ==========
        private readonly string connectionString =
            "Data Source=LAPTOP-SDC5DOB7\\EGIN;Initial Catalog=DBakademikADO;Integrated Security=True";

        // DataTable untuk menyimpan data (sesuai modul)
        private DataTable dtMahasiswa;
        private BindingSource bindingSource;

        public Form1()
        {
            InitializeComponent();
            bindingSource = new BindingSource();
        }

        // ========== LANGKAH 6: CONNECT TEST ==========
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
                MessageBox.Show("❌ Koneksi ke database GAGAL!\n\n" +
                    "Pastikan:\n" +
                    "1. SQL Server sedang berjalan\n" +
                    "2. Database 'DBakademikADO' sudah dibuat\n" +
                    "3. VIEW 'vwMahasiswaPublic' sudah dibuat\n\n" +
                    "Error Detail: " + ex.Message,
                    "Error Koneksi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            ConnectDatabase();
        }

        // ========== LANGKAH 3 & 4: LOAD DATA DARI VIEW ==========
        private void LoadData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // ========== MENGGUNAKAN VIEW (SESUAI MODUL LANGKAH 1 & 4) ==========
                    string query = "SELECT * FROM vwMahasiswaPublic ORDER BY NIM";

                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        dtMahasiswa = new DataTable();
                        da.Fill(dtMahasiswa);

                        bindingSource.DataSource = dtMahasiswa;
                        dataGridView1.DataSource = bindingSource;
                        bindingNavigator1.BindingSource = bindingSource;

                        // ========== LANGKAH 5: BIND CONTROLS ==========
                        BindControls();

                        lblStatus.Text = $"✅ Berhasil memuat {dtMahasiswa.Rows.Count} data mahasiswa (via VIEW)";
                        lblStatus.ForeColor = System.Drawing.Color.Green;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "❌ Gagal memuat data";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
        }

        // ========== LANGKAH 5: BIND CONTROLS ==========
        private void BindControls()
        {
            // Clear existing bindings
            txtNIM.DataBindings.Clear();
            txtNama.DataBindings.Clear();
            cmbJK.DataBindings.Clear();
            dtpTanggalLahir.DataBindings.Clear();
            txtAlamat.DataBindings.Clear();
            txtKodeProdi.DataBindings.Clear();

            // Add new bindings ke BindingSource
            txtNIM.DataBindings.Add("Text", bindingSource, "NIM");
            txtNama.DataBindings.Add("Text", bindingSource, "Nama");
            cmbJK.DataBindings.Add("Text", bindingSource, "JenisKelamin");
            dtpTanggalLahir.DataBindings.Add("Value", bindingSource, "TanggalLahir");
            txtAlamat.DataBindings.Add("Text", bindingSource, "Alamat");
            txtKodeProdi.DataBindings.Add("Text", bindingSource, "KodeProdi");
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
            if (dataGridView1.Columns["KodeProdi"] != null)
                dataGridView1.Columns["KodeProdi"].HeaderText = "Kode Prodi";
        }

        // ========== LANGKAH 7: TOMBOL LOAD ==========
        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        // ========== LANGKAH 8: INSERT AMAN (PARAMETERIZED QUERY) ==========
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
            if (string.IsNullOrWhiteSpace(txtKodeProdi.Text))
            {
                MessageBox.Show("❌ Kode Prodi harus diisi!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtKodeProdi.Focus();
                return;
            }

            // Validasi panjang karakter
            if (txtNIM.Text.Trim().Length < 10)
            {
                MessageBox.Show("❌ NIM minimal 10 karakter!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNIM.Focus();
                return;
            }
            if (txtNama.Text.Trim().Length < 3)
            {
                MessageBox.Show("❌ Nama minimal 3 karakter!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNama.Focus();
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // ========== PARAMETERIZED QUERY (AMAN DARI SQL INJECTION) ==========
                    string query = @"INSERT INTO Mahasiswa 
                                    (NIM, Nama, JenisKelamin, Tanggallahir, Alamat, KodeProdi, TanggallDaftar) 
                                    VALUES 
                                    (@NIM, @Nama, @JK, @TanggalLahir, @Alamat, @KodeProdi, @TanggalDaftar)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Menggunakan parameter (AMAN)
                        cmd.Parameters.AddWithValue("@NIM", txtNIM.Text.Trim());
                        cmd.Parameters.AddWithValue("@Nama", txtNama.Text.Trim());
                        cmd.Parameters.AddWithValue("@JK", cmbJK.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@TanggalLahir", dtpTanggalLahir.Value.Date);
                        cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text.Trim());
                        cmd.Parameters.AddWithValue("@KodeProdi", txtKodeProdi.Text.Trim());
                        cmd.Parameters.AddWithValue("@TanggalDaftar", DateTime.Now);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("✅ Data mahasiswa berhasil ditambahkan!", "Sukses",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            ClearForm();
                            LoadData(); // Refresh data
                        }
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                MessageBox.Show("❌ NIM sudah terdaftar! Gunakan NIM yang berbeda.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtNIM.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ========== UPDATE AMAN (PARAMETERIZED QUERY) ==========
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNIM.Text))
            {
                MessageBox.Show("❌ Pilih data yang akan diupdate terlebih dahulu!",
                    "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        conn.Open();

                        // ========== PARAMETERIZED QUERY (AMAN) ==========
                        string query = @"UPDATE Mahasiswa 
                                        SET Nama = @Nama, 
                                            JenisKelamin = @JK, 
                                            Tanggallahir = @TanggalLahir, 
                                            Alamat = @Alamat, 
                                            KodeProdi = @KodeProdi 
                                        WHERE NIM = @NIM";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@NIM", txtNIM.Text.Trim());
                            cmd.Parameters.AddWithValue("@Nama", txtNama.Text.Trim());
                            cmd.Parameters.AddWithValue("@JK", cmbJK.SelectedItem.ToString());
                            cmd.Parameters.AddWithValue("@TanggalLahir", dtpTanggalLahir.Value.Date);
                            cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text.Trim());
                            cmd.Parameters.AddWithValue("@KodeProdi", txtKodeProdi.Text.Trim());

                            int result = cmd.ExecuteNonQuery();

                            if (result > 0)
                            {
                                MessageBox.Show("✅ Data berhasil diupdate!", "Sukses",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearForm();
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("❌ Data tidak ditemukan!", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        }

        // ========== DELETE AMAN (PARAMETERIZED QUERY) ==========
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNIM.Text))
            {
                MessageBox.Show("❌ Pilih data yang akan dihapus terlebih dahulu!",
                    "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"⚠️ Yakin ingin menghapus data mahasiswa dengan NIM {txtNIM.Text}?\n\nData yang dihapus tidak dapat dikembalikan!",
                "Konfirmasi Hapus",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        string query = "DELETE FROM Mahasiswa WHERE NIM = @NIM";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@NIM", txtNIM.Text.Trim());

                            int result = cmd.ExecuteNonQuery();

                            if (result > 0)
                            {
                                MessageBox.Show("✅ Data berhasil dihapus!", "Sukses",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearForm();
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("❌ Data tidak ditemukan!", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                txtKodeProdi.Text = row.Cells["KodeProdi"].Value.ToString();

                lblStatus.Text = $"📌 Terpilih: {txtNama.Text} ({txtNIM.Text})";
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
            txtKodeProdi.Clear();
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

        // ========== LANGKAH 9: RESET DATA (Backup & Restore) ==========
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

                        LoadData(); // Refresh data
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("❌ Reset gagal: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ========== LANGKAH 10: SIMULASI SQL INJECTION (KONSEPTUAL) ==========
        // PERINGATAN: Kode ini hanya untuk DEMONSTRASI bahaya SQL Injection!
        private void btnTestInjection_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNIM.Text))
            {
                MessageBox.Show("❌ Masukkan NIM untuk uji injection!\nContoh: ' OR 1=1 -- -",
                    "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // ========== QUERY TIDAK AMAN (RENTAN SQL INJECTION) ==========
                    // Jangan pernah gunakan cara ini di aplikasi nyata!
                    string query = "UPDATE Mahasiswa SET Nama='HACKED' WHERE NIM='" + txtNIM.Text + "'";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        int result = cmd.ExecuteNonQuery();
                        MessageBox.Show(result + " baris terupdate! Data mungkin telah berubah!",
                            "⚠️ Hasil Injection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    LoadData(); // Refresh untuk melihat efeknya
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
            // Setup ComboBox JK (sesuai modul)
            cmbJK.DataSource = new string[] { "L", "P" };
            cmbJK.DropDownStyle = ComboBoxStyle.DropDownList;

            // Setup DataGridView
            SetupDataGridView();

            // Setup BindingNavigator
            bindingNavigator1.BindingSource = bindingSource;

            // Load data
            LoadData();

            lblStatus.Text = "✅ Aplikasi siap digunakan. Klik 'Membuka Koneksi' untuk test koneksi.";
            lblStatus.ForeColor = System.Drawing.Color.Green;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Optional: kosong
        }
    }
}