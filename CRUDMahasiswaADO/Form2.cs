using System;
using System.Data;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;

namespace CRUDMahasiswaADO
{
    public partial class Form2 : Form
    {
        // ========== DEKLARASI OBJEK DAL ==========
        DAL dbLogic = new DAL();

        private string prodi;
        private DateTime tglmasuk;

        // ========== 16. UBAH CONSTRUCTOR REPORT ==========
        public Form2(string Prodi, DateTime TglMasuk)
        {
            InitializeComponent();
            prodi = Prodi;
            tglmasuk = TglMasuk;

            try
            {
                DataTable dtMahasiswa = dbLogic.GetDataRekap(prodi, tglmasuk);

                // Gunakan CrystalReport1 (sesuai dengan nama report di project)
                CrystalReport1 report = new CrystalReport1();
                report.SetDataSource(dtMahasiswa);
                crystalReportViewer1.ReportSource = report;
                crystalReportViewer1.Refresh();
            }
            catch (Exception ex)
            {
                // simpanLog(ex.Message);
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void crystalReportViewer1_Load(object sender, EventArgs e)
        {

        }
    }
}