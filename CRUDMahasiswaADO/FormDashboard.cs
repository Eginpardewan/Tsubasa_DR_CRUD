using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CRUDMahasiswaADO
{
    public partial class FormDashboard : Form
    {
        DAL dbLogic = new DAL();
        bool isInitializing = true;
        DataTable dt;
        int button = 0;

        public FormDashboard()
        {
            InitializeComponent();
        }

        private void FormDashboard_Load(object sender, EventArgs e)
        {
            // ========== ISI COMBOBOX TAHUN ==========
            cmbTahun.Items.Clear();
            for (int i = 2020; i <= DateTime.Now.Year; i++)
            {
                cmbTahun.Items.Add(i.ToString());
            }
            if (cmbTahun.Items.Count > 0)
                cmbTahun.SelectedIndex = cmbTahun.Items.Count - 1;

            cmbTipe.DropDownStyle = ComboBoxStyle.DropDownList;
            var items = new List<KeyValuePair<string, SeriesChartType>>
            {
                new KeyValuePair<string, SeriesChartType>("Kolom", SeriesChartType.Column),
                new KeyValuePair<string, SeriesChartType>("Pie", SeriesChartType.Pie)
            };

            isInitializing = true;

            cmbTipe.DataSource = items;
            cmbTipe.DisplayMember = "Key";
            cmbTipe.ValueMember = "Value";
            cmbTipe.SelectedIndex = 0;

            isInitializing = false;
            LoadDataChart();
        }

        public void LoadDataChart()
        {
            chartProdi.Series.Clear();
            chartProdi.Titles.Clear();
            chartProdi.Legends.Clear();
            chartProdi.ChartAreas.Clear();

            ChartArea ca = new ChartArea("MainArea");
            ca.AxisX.Title = "Program Studi";
            ca.AxisY.Title = "Jumlah Mahasiswa";
            ca.AxisX.LabelStyle.Angle = -45;
            ca.BackColor = Color.Transparent;
            chartProdi.ChartAreas.Add(ca);

            try
            {
                if (button == 1)
                {
                    string tahun = cmbTahun.SelectedItem?.ToString();
                    if (!string.IsNullOrEmpty(tahun))
                    {
                        dt = dbLogic.GetDataChartByTahun(Convert.ToInt32(tahun));  // ← PERBAIKI INI
                    }
                    else
                    {
                        dt = dbLogic.GetAllDataChart();
                    }
                }
                else
                {
                    dt = dbLogic.GetAllDataChart();
                }

                // Jika tidak ada data, keluar dari method
                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk ditampilkan.", "Informasi",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SeriesChartType tipe = (SeriesChartType)cmbTipe.SelectedValue;

                Series s = new Series("Jumlah Mahasiswa");
                s.ChartType = tipe;
                s.IsValueShownAsLabel = true;
                s.Label = "#VAL";
                s.LegendText = "#VAL";

                foreach (DataRow row in dt.Rows)
                {
                    string prodi = row["NamaProdi"].ToString();
                    int jumlah = Convert.ToInt32(row["JmlhMhs"]);
                    s.Points.AddXY(prodi, jumlah);
                }

                chartProdi.Series.Add(s);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message);
            }

            Title title = new Title("Jumlah Mahasiswa per Program Studi", Docking.Top, new Font("Arial", 14, FontStyle.Bold), Color.DarkBlue);
            chartProdi.Titles.Add(title);

            Legend legend = new Legend("MainLegend");
            legend.Docking = Docking.Right;
            chartProdi.Legends.Add(legend);
        }

        private void cmbTipe_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isInitializing)
            {
                LoadDataChart();
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (cmbTahun.SelectedItem == null)
            {
                MessageBox.Show("Pilih tahun terlebih dahulu!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            button = 1;
            LoadDataChart();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            button = 0;
            LoadDataChart();
        }

        private void btnDataMahasiswa_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }
    }
}