using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace DBManager
{
    class SQLServerDBConnection
    {
        private SqlConnection _connSQLServer;

        public SQLServerDBConnection(string SQLServerConnectionInfo)
        {
            _connSQLServer = new SqlConnection();
            _connSQLServer.ConnectionString = SQLServerConnectionInfo;
        }
        public bool OpenDBConnection()
        {
            try
            {
                if (_connSQLServer.State == ConnectionState.Closed)
                    _connSQLServer.Open();
                return true;
            }
            catch (SqlException e)
            {
                MessageBox.Show("Connection failed: " + e.Message);
                return false;
            }
        }
        public DataTable GetDatabases()
        {
            SqlCommand cmd = new SqlCommand("SELECT name from sys.databases", _connSQLServer);
            DataTable dtRecord = new DataTable();
            if (OpenDBConnection())
            {
                try
                {
                    SqlDataAdapter sqlDataAdap = new SqlDataAdapter(cmd);
                    sqlDataAdap.Fill(dtRecord);
                }
                catch (SqlException e)
                {
                    MessageBox.Show("Unable to fill database table: " + e.Message);
                }
                _connSQLServer.Close();
            }
            return dtRecord;
        }
        public void CreateDatabase(string dbname)
        {
            SqlCommand cmd = new SqlCommand("CREATE DATABASE [" + dbname + "]", _connSQLServer);
            if (OpenDBConnection())
            {
                try 
                { 
                    cmd.ExecuteNonQuery();
                    MessageBox.Show(dbname + " created successfully.");
                }
                catch (SqlException e) 
                { 
                    MessageBox.Show("Database creation failed: " + e.Message); 
                }
                _connSQLServer.Close();
            }
        }
        public void BackupDatabase(string dbname, string diskpath)
        {
            SqlCommand cmd = new SqlCommand(
                string.Format(@"USE [{0}] 
                                BACKUP DATABASE [{0}] 
                                TO DISK = '{1}'", dbname, diskpath), _connSQLServer);
            if (OpenDBConnection())
            {
                try
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show(dbname + " backup successful."); 
                }
                catch (SqlException e)
                {
                    MessageBox.Show("Backup failed: " + e.Message);
                }
                _connSQLServer.Close();
            }
        }
        public string GetDefaultBackupPath()
        {
            SqlCommand cmd = new SqlCommand("EXEC  master.dbo.xp_instance_regread  N'HKEY_LOCAL_MACHINE', N'Software\\Microsoft\\MSSQLServer\\MSSQLServer',N'BackupDirectory'",
                                            _connSQLServer);
            string path = "";
            if (OpenDBConnection())
            {
                try
                {
                    SqlDataReader myDataReader = cmd.ExecuteReader();
                    myDataReader.Read();
                    path = myDataReader.GetString(1); 
                    myDataReader.Close();
                }
                catch (SqlException e)
                {
                    MessageBox.Show("Unable to get default backup path: " + e.Message);
                }
                _connSQLServer.Close();
            }
            return path;
        }
        public DataTable GetBackupHistory()
        {
            SqlCommand cmd = new SqlCommand(@"SELECT TOP 50 p.database_name AS 'Database Name',
                                              p.backup_finish_date AS 'Date Created',
                                              a.physical_device_name AS 'Path'
                                              FROM msdb..backupmediafamily a,
                                              msdb..backupset p
                                              WHERE a.media_set_id=p.media_set_id
                                              ORDER BY p.backup_finish_date DESC", _connSQLServer);
            DataTable dtRecord = new DataTable();
            if (OpenDBConnection())
            {
                try
                {
                    SqlDataAdapter sqlDataAdap = new SqlDataAdapter(cmd);
                    sqlDataAdap.Fill(dtRecord);
                }
                catch (SqlException e)
                {
                    MessageBox.Show("Unable to fill backups table: " + e.Message);
                }
                _connSQLServer.Close();
            }
            return dtRecord;
        }
        public void RestoreDatabase(string dbname, string diskpath)
        {
            SqlCommand cmd = new SqlCommand(
                string.Format(@"RESTORE DATABASE [{0}] 
                                FROM DISK = '{1}'
                                WITH REPLACE", dbname, diskpath), _connSQLServer);
            if (OpenDBConnection())
            {
                try
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show(dbname + " restored successfully.");
                }
                catch (SqlException e)
                {
                    MessageBox.Show("Restoration failed: " + e.Message);
                }
                _connSQLServer.Close();
            }
        }
        public DataTable ListStoredProcedures(string dbname)
        {
            SqlCommand cmd = new SqlCommand(
                string.Format(@"USE [{0}]
                                SELECT sp.name AS 'Name' FROM sys.procedures sp", dbname), _connSQLServer);
            DataTable dtRecord = new DataTable();
            if (OpenDBConnection())
            {
                try
                {
                    SqlDataAdapter sqlDataAdap = new SqlDataAdapter(cmd);
                    sqlDataAdap.Fill(dtRecord);
                }
                catch (SqlException e)
                {
                    MessageBox.Show("Unable to list stored procedures: " + e.Message);
                }
                _connSQLServer.Close();
            }
            return dtRecord;
        }
        public string SPText(string dbname, string spname)
        {
            SqlCommand cmd = new SqlCommand(
                string.Format(@"USE [{0}]
                                EXEC sp_helptext '{1}'", dbname, spname), _connSQLServer);
            string text = "";
            if (OpenDBConnection())
            {
                try
                {
                    SqlDataReader DataReader = cmd.ExecuteReader();
                    while (DataReader.Read()) text += DataReader.GetString(0) + "\n";
                    DataReader.Close();
                    _connSQLServer.Close();
                }
                catch (SqlException e)
                {
                    MessageBox.Show("Unable to get stored procedure text: " + e.Message);
                }
                _connSQLServer.Close();
            }
            return text;
        }
        public void Encrypt(string dbname, string spname)
        {
            //string cmdtxt = string.Format(@"USE {0};
//{1}", dbname, SPText(dbname, spname));
            string cmdtxt = SPText(dbname, spname);
            cmdtxt = Regex.Replace(cmdtxt, @"\bCREATE\b", "ALTER", RegexOptions.IgnoreCase);
            Regex rgx = new Regex(@"\b(AS|as)\b");
            cmdtxt = rgx.Replace(cmdtxt, "WITH ENCRYPTION AS", 1);
            SqlCommand cmd = new SqlCommand(cmdtxt, _connSQLServer);
            //MessageBox.Show(cmdtxt);
            if (OpenDBConnection())
            {
                try
                {
                    _connSQLServer.ChangeDatabase(dbname);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show(spname + " has been encrypted.");
                }
                catch (SqlException e)
                {
                    MessageBox.Show("Unable to encrypt: " + e.Message);
                }
                _connSQLServer.Close();
            }
        }
    }
}
