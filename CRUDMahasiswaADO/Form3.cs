using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class Form3 : Form
    {
        // ========== 15. DEKLARASI OBJEK DAL ==========
        DAL dbLogic = new DAL();

        // ========== KONEKSI DATABASE (Opsional jika masih menggunakan conn langsung) ==========
        static string connectionString = "Data Source=LAPTOP-SDC5DOB7\\EGIN;Initial Catalog=DBakademikADO;Integrated Security=True";
        SqlConnection conn = new SqlConnection(connectionString);
        SqlDataAdapter da;
        DataTable dtMahasiswa;
        DataTable dtProdi;

        public Form3()
        {
            InitializeComponent();
        }

        // ========== FORM LOAD ==========
        private void Form3_Load(object sender, EventArgs e)
        {
            dtpTanggalMasuk.Format = DateTimePickerFormat.Custom;
            dtpTanggalMasuk.CustomFormat = "yyyy";
            dtpTanggalMasuk.ShowUpDown = true;
            dtpTanggalMasuk.MinDate = new DateTime(2000, 1, 1);
            dtpTanggalMasuk.MaxDate = DateTime.Now;

            cmbProdi.DropDownStyle = ComboBoxStyle.DropDownList;

            btnCetak.Enabled = false;

            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                // Gunakan dbLogic untuk mengambil data Prodi
                DataTable dtProdi = dbLogic.GetProdi();
                cmbProdi.DataSource = dtProdi;
                cmbProdi.DisplayMember = "NamaProdi";
                cmbProdi.ValueMember = "NamaProdi";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        // ========== btnLoad_CLICK ==========
        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                SqlCommand cmd = new SqlCommand("sp_Report", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@inProdi", cmbProdi.SelectedValue.ToString());
                cmd.Parameters.AddWithValue("@inTgLMsuK", dtpTanggalMasuk.Value.Year.ToString());

                da = new SqlDataAdapter(cmd);
                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);

                dataGridView1.DataSource = dtMahasiswa;

                if (dtMahasiswa.Rows.Count > 0)
                {
                    btnCetak.Enabled = true;
                }
                else
                {
                    btnCetak.Enabled = false;
                    MessageBox.Show("Tidak ada data untuk kriteria yang dipilih.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        // ========== btnCetak_CLICK (Panggil Form2) ==========
        private void btnCetak_Click(object sender, EventArgs e)
        {
            // Kirim DateTime (bukan int tahun) - sesuai dengan constructor Form2
            Form2 frm2 = new Form2(cmbProdi.SelectedValue.ToString(), dtpTanggalMasuk.Value);
            frm2.Show();
            this.Hide();
        }
    }
}