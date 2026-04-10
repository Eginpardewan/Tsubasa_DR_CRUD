using System;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class Form1 : Form
    {
        private readonly SqlConnection conn;
        private readonly string connectionString =
            "Data Source=DESKTOP-RAM20FI\\APRILIYA;Initial Catalog=DBAkademikADO;Integrated Security=True";
        public Form1()
        {
            InitializeComponent();
            conn = new SqlConnection(connectionString);
            cmbJK.Items.Add("L");
            cmbJK.Items.Add("P");
        }

        // ========== Method Koneksi Database ==========
        private void ConnectDatabase()
        {
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                MessageBox.Show("Koneksi berhasil");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Koneksi gagal: " + ex.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        // ========== Event Tombol Koneksi ==========
        private void btnConnect_Click(object sender, EventArgs e)
        {
            ConnectDatabase();
        }

        
    }
}