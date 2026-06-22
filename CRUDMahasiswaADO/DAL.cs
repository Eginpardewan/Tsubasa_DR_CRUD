using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public class DAL
    {
        private static string connectionString = "Data Source=LAPTOP-SDC5DOB7\\EGIN;Initial Catalog=DBakademikADO;Integrated Security=True;";
        private SqlConnection conn;
        private SqlDataAdapter da;
        private DataTable dtMahasiswa;
        private DataTable dtProdi;

        public DAL()
        {
            conn = new SqlConnection(connectionString);
        }

        // ========== GET CONNECTION STRING ==========
        public string GetConnectionString()
        {
            return connectionString;
        }

        // ========== OPEN CONNECTION ==========
        private void OpenConnection()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
        }

        // ========== CLOSE CONNECTION ==========
        private void CloseConnection()
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }

        // ========== 1. COUNT MAHASISWA ==========
        public int CountMhs()
        {
            try
            {
                OpenConnection();
                SqlCommand cmd = new SqlCommand("sp_CountMahasiswa", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlParameter outputParam = new SqlParameter("@Total", SqlDbType.Int);
                outputParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(outputParam);

                cmd.ExecuteNonQuery();
                return Convert.ToInt32(outputParam.Value);
            }
            catch (Exception ex)
            {
                InsertLog(ex.Message);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }
        }

        // ========== 2. GET ALL MAHASISWA ==========
        public DataTable GetMhs()
        {
            try
            {
                OpenConnection();
                SqlCommand cmd = new SqlCommand("sp_GetMahasiswa", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                da = new SqlDataAdapter(cmd);
                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);

                return dtMahasiswa;
            }
            catch (Exception ex)
            {
                InsertLog(ex.Message);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }
        }

        // ========== 3. GET MAHASISWA BY NIM ==========
        public DataTable GetMhsByNIM(string nim)
        {
            try
            {
                OpenConnection();
                SqlCommand cmd = new SqlCommand("sp_GetMahasiswaByNIM", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@pNIM", nim);

                da = new SqlDataAdapter(cmd);
                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);

                return dtMahasiswa;
            }
            catch (Exception ex)
            {
                InsertLog(ex.Message);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }
        }

        // ========== 4. INSERT MAHASISWA ==========
        public void InsertMhs(string nim, string nama, string alamat, string jenisKelamin, DateTime tanggalLahir, string kodeProdi, byte[] foto)
        {
            try
            {
                OpenConnection();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    SqlCommand cmd = new SqlCommand("sp_InsertMahasiswa", conn, trans);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@pNIM", nim);
                    cmd.Parameters.AddWithValue("@pNama", nama);
                    cmd.Parameters.AddWithValue("@pAlamat", alamat);
                    cmd.Parameters.AddWithValue("@pJenisKelamin", jenisKelamin);
                    cmd.Parameters.AddWithValue("@pTanggallahir", tanggalLahir);
                    cmd.Parameters.AddWithValue("@pKodeProdi", kodeProdi);
                    cmd.Parameters.AddWithValue("@pFoto", foto);

                    cmd.ExecuteNonQuery();
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    InsertLog(ex.Message);
                    throw ex;
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        // ========== 5. UPDATE MAHASISWA ==========
        public void UpdateMhs(string nim, string nama, string alamat, string jenisKelamin, DateTime tanggalLahir, string kodeProdi, byte[] foto)
        {
            try
            {
                OpenConnection();
                SqlCommand cmd = new SqlCommand("sp_UpdateMahasiswa", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@pNIM", nim);
                cmd.Parameters.AddWithValue("@pNama", nama);
                cmd.Parameters.AddWithValue("@pAlamat", alamat);
                cmd.Parameters.AddWithValue("@pJenisKelamin", jenisKelamin);
                cmd.Parameters.AddWithValue("@pTanggallahir", tanggalLahir);
                cmd.Parameters.AddWithValue("@pKodeProdi", kodeProdi);
                cmd.Parameters.AddWithValue("@pFoto", foto);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                InsertLog(ex.Message);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }
        }

        // ========== 6. DELETE MAHASISWA ==========
        public void DeleteMhs(string nim)
        {
            try
            {
                OpenConnection();
                SqlCommand cmd = new SqlCommand("sp_DeleteMahasiswa", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@pNIM", nim);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                InsertLog(ex.Message);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }
        }

        // ========== 7. RESET DATA ==========
        public void ResetData()
        {
            try
            {
                OpenConnection();

                string deleteQuery = "DELETE FROM Mahasiswa;";
                SqlCommand cmdDelete = new SqlCommand(deleteQuery, conn);
                cmdDelete.ExecuteNonQuery();

                string insertQuery = @"INSERT INTO Mahasiswa SELECT * FROM Mahasiswa_Backup;";
                SqlCommand cmdInsert = new SqlCommand(insertQuery, conn);
                cmdInsert.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                InsertLog(ex.Message);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }
        }

        // ========== 8. TEST INJECTION ==========
        public void TestInject(string nim)
        {
            try
            {
                OpenConnection();
                string query = "UPDATE Mahasiswa SET Nama = 'HACKED' WHERE NIM = '" + nim + "'";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                InsertLog(ex.Message);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }
        }

        // ========== 9. INSERT LOG ==========
        public void InsertLog(string message)
        {
            try
            {
                OpenConnection();
                SqlCommand cmd = new SqlCommand("INSERT INTO LogError (waktu, pesan_error) VALUES (GETDATE(), @pesan)", conn);
                cmd.Parameters.AddWithValue("@pesan", message);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan log: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
        }

        // ========== 10. GET PRODI ==========
        public DataTable GetProdi()
        {
            try
            {
                OpenConnection();
                SqlCommand cmd = new SqlCommand("SELECT NamaProdi FROM ProgramStudi", conn);
                cmd.CommandType = CommandType.Text;
                dtProdi = new DataTable();
                da = new SqlDataAdapter(cmd);
                da.Fill(dtProdi);

                return dtProdi;
            }
            catch (Exception ex)
            {
                InsertLog(ex.Message);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }
        }

        // ========== 11. GET DATA REKAP ==========
        public DataTable GetDataRekap(string prodi, DateTime tanggalMasuk)
        {
            try
            {
                OpenConnection();
                SqlCommand cmd = new SqlCommand("sp_Report", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@inProdi", prodi);
                cmd.Parameters.AddWithValue("@inTgLMsuK", tanggalMasuk.Year.ToString());

                da = new SqlDataAdapter(cmd);
                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);
                return dtMahasiswa;
            }
            catch (Exception ex)
            {
                InsertLog(ex.Message);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }
        }

        // ========== 12. GET ALL DATA CHART ==========
        public DataTable GetAllDataChart()
        {
            try
            {
                OpenConnection();
                SqlCommand cmd = new SqlCommand("sp_Dashboard", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                da = new SqlDataAdapter(cmd);
                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);
                return dtMahasiswa;
            }
            catch (Exception ex)
            {
                InsertLog(ex.Message);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }
        }

        // ========== 13. GET DATA CHART BY TAHUN (DATETIME) ==========
        public DataTable GetDataChartByTahun(DateTime thMasuk)
        {
            try
            {
                OpenConnection();
                SqlCommand cmd = new SqlCommand("sp_DashboardByTahun", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intTglMsuk", thMasuk.Year.ToString());

                da = new SqlDataAdapter(cmd);
                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);
                return dtMahasiswa;
            }
            catch (Exception ex)
            {
                InsertLog(ex.Message);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }
        }

        // ========== 13a. GET DATA CHART BY TAHUN (INT) ==========
        // Method ini dipanggil dari FormDashboard dengan parameter int (tahun dari ComboBox)
        public DataTable GetDataChartByTahun(int tahun)
        {
            try
            {
                OpenConnection();
                SqlCommand cmd = new SqlCommand("sp_DashboardByTahun", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@intTglMsuk", tahun.ToString());

                da = new SqlDataAdapter(cmd);
                dtMahasiswa = new DataTable();
                da.Fill(dtMahasiswa);
                return dtMahasiswa;
            }
            catch (Exception ex)
            {
                InsertLog(ex.Message);
                throw ex;
            }
            finally
            {
                CloseConnection();
            }
        }

        // ========== 14. CEK KONEKSI ==========
        public bool TestConnection()
        {
            try
            {
                OpenConnection();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }
    }
}