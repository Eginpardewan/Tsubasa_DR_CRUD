using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;

namespace CRUDMahasiswaADO
{
    public partial class Form2 : Form
    {
        static string connectionString = "Data Source=LAPTOP-SDC5DOB7\\EGIN;Initial Catalog=DBakademikADO;Integrated Security=True";
        private string prodi;
        private int tahun;

        public Form2(string prodi, int tahun)
        {
            InitializeComponent();
            this.prodi = prodi;
            this.tahun = tahun;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            List<Data> listData = new List<Data>();  // ← menggunakan class Data

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_Report", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@inProdi", prodi);
                cmd.Parameters.AddWithValue("@inTgLMsuK", tahun.ToString());

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Data data = new Data();
                    data.Nama = reader["Nama"].ToString();
                    data.JenisKelamin = reader["JenisKelamin"].ToString();
                    data.Alamat = reader["Alamat"].ToString();
                    data.NamaProdi = reader["NamaProdi"].ToString();
                    data.TanggalDaftar = Convert.ToDateTime(reader["TanggalDaftar"]);
                    listData.Add(data);
                }
            }

            CrystalReport1 report = new CrystalReport1();
            report.SetDataSource(listData);
            crystalReportViewer1.ReportSource = report;
            crystalReportViewer1.Refresh();
        }

        private void crystalReportViewer1_Load(object sender, EventArgs e)
        {

        }
    }
}