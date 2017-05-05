using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBManager
{
    public partial class Form1 : Form
    {
        SQLServerDBConnection DBConn;

        public Form1()
        {
            InitializeComponent();
            comboBox1.Items.Add("Windows Authentication");
            comboBox1.Items.Add("Standard Connection");
            comboBox1.SelectedIndex = 0;
        }
        //Connect
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
                DBConn = new SQLServerDBConnection("Data Source=" + textBox1.Text + ";" +
                                                   "Integrated Security=SSPI;");
            else 
                DBConn = new SQLServerDBConnection("Data Source=" + textBox1.Text + ";" +
                                                   "User id=" + textBox2.Text + ";" +
                                                   "Password=" + textBox3.Text + ";");
            if (DBConn.OpenDBConnection())
            {
                DBView.DataSource = DBConn.GetDatabases();
                BackupView.DataSource = DBConn.GetBackupHistory();
                buttonCreateDB.Enabled = true;
                buttonRestore.Enabled = true;
                buttonBackup.Enabled = (DBView.Rows.Count > 0);
                buttonEncryptSP.Enabled = (SPNameView.Rows.Count > 0);
            }
            else
            {
                BackupView.DataSource = null;
                DBView.DataSource = null;
                buttonCreateDB.Enabled = false;
                buttonRestore.Enabled = false;
                buttonBackup.Enabled = false;
            }
        }
        //Windows Authentication or Username/Password
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool b = (comboBox1.SelectedIndex == 1);
            label2.Enabled = b;
            label3.Enabled = b;
            textBox2.Enabled = b;
            textBox3.Enabled = b;
        }
        //Create database
        private void button2_Click(object sender, EventArgs e)
        {
            string dbname = Microsoft.VisualBasic.Interaction.InputBox("Enter a name for the database to be created:", 
                                                                       "Create Database", "Default");
            if (dbname.Length > 0)
            {
                DBConn.CreateDatabase(dbname);
                DBView.DataSource = DBConn.GetDatabases();
                buttonBackup.Enabled = (DBView.RowCount > 0);
            }
        }
        //Backup database
        private void button3_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = DBView.Rows[DBView.SelectedRows[0].Index];
            string dbname = row.Cells[0].Value.ToString();
            string filename = Microsoft.VisualBasic.Interaction.InputBox(dbname + " was selected. Enter a name for the .bak file:",
                                                                             "Backup Database", dbname);
            if (filename.Length > 0)
            {
                FolderBrowserDialog folderDialog = new FolderBrowserDialog();
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string diskpath = string.Format(@"{0}\{1}.bak", folderDialog.SelectedPath, filename);
                    DBConn.BackupDatabase(dbname, diskpath);
                    BackupView.DataSource = DBConn.GetBackupHistory();
                }
            }
        }
        //Restore database
        private void button1_Click_1(object sender, EventArgs e)
        {
            DataGridViewRow row = BackupView.Rows[BackupView.SelectedRows[0].Index];
            string dbname = row.Cells[0].Value.ToString();
            string diskpath = row.Cells[2].Value.ToString();
            DBConn.RestoreDatabase(dbname, diskpath);
        }
        //Show Stored Procedures
        private void DBView_SelectionChanged(object sender, EventArgs e)
        {
            if (DBView.SelectedRows.Count > 0)
            {
                DataGridViewRow row = DBView.Rows[DBView.SelectedRows[0].Index];
                string dbname = row.Cells[0].Value.ToString();
                SPNameView.DataSource = DBConn.ListStoredProcedures(dbname);
            }
            else
            {
                SPNameView.DataSource = null;
                SPBox.Text = "";
            }
        }
        //Get SP Text
        private void SPNameView_SelectionChanged(object sender, EventArgs e)
        {
            if (SPNameView.SelectedRows.Count > 0)
            {
                buttonEncryptSP.Enabled = true;
                DataGridViewRow row1 = DBView.Rows[DBView.SelectedRows[0].Index];
                DataGridViewRow row2 = SPNameView.Rows[SPNameView.SelectedRows[0].Index];
                string dbname = row1.Cells[0].Value.ToString();
                string spname = row2.Cells[0].Value.ToString();
                SPBox.Text = DBConn.SPText(dbname, spname);
            }
            else
            {
                buttonEncryptSP.Enabled = false;
                SPBox.Text = "";
            }
            if (SPBox.Text == "") buttonEncryptSP.Enabled = false;
        }
        //Encrypt SP
        private void buttonEncryptSP_Click(object sender, EventArgs e)
        {
            DataGridViewRow row1 = DBView.Rows[DBView.SelectedRows[0].Index];
            DataGridViewRow row2 = SPNameView.Rows[SPNameView.SelectedRows[0].Index];
            string dbname = row1.Cells[0].Value.ToString();
            string spname = row2.Cells[0].Value.ToString();
            DBConn.Encrypt(dbname, spname);
            buttonEncryptSP.Enabled = false;
            SPBox.Text = "";
        }
    }
}
