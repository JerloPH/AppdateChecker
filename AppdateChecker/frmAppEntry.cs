using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AppdateChecker;

namespace AppdateChecker
{
    public partial class frmAppEntry : Form
    {
        public Dictionary<string, string> Result { get; set; } = null;
        private string Uid = "";

        public frmAppEntry(Dictionary<string, string> Entry)
        {
            InitializeComponent();
            if (Entry == null)
            {
                this.Uid = "0";
                Text = "Add New App entry";
                Result = new Dictionary<string, string>();
                InitiateDefaultValues();
            }
            else
            {
                Result = Entry;
                Text = "Edit App Entry";
            }
        }
        private void InitiateDefaultValues()
        {
            Result.Add(SQLHelper.DbColId, "");
            Result.Add(SQLHelper.DbColName, "");
            Result.Add(SQLHelper.DbColRepoOwner, "");
            Result.Add(SQLHelper.DbColRepoName, "");
            Result.Add(SQLHelper.DbColCurVer, "");
            Result.Add(SQLHelper.DbColFilepath, "");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Result?.Clear();
            Result = null;
            Close();
        }
        private void btnOk_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtName.Text))
            {
                txtName.Focus();
                return;
            }
            if (String.IsNullOrWhiteSpace(txtRepoOwner.Text) || String.IsNullOrWhiteSpace(txtRepoName.Text))
            {
                return;
            }

            Result[SQLHelper.DbColId] = this.Uid;
            Result[SQLHelper.DbColName] = txtName.Text;
            Result[SQLHelper.DbColRepoOwner] = txtRepoOwner.Text;
            Result[SQLHelper.DbColRepoName] = txtRepoName.Text;
            Result[SQLHelper.DbColCurVer] = txtAppVersion.Text;
            Result[SQLHelper.DbColFilepath] = txtPath.Text;
            Close();
        }

        private void txtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtUrl.Focus();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            string file = GlobalFunc.GetAFile("Select Executable file", "Executable (*.exe)|*.exe;", AppContext.BaseDirectory);
            if (!String.IsNullOrWhiteSpace(file))
                txtPath.Text = file;
        }

        private void btnDetect_Click(object sender, EventArgs e)
        {
            // Auto-detect repo owner and name from given URL, and App version from file given
            string Url = txtUrl.Text;
            string file = txtPath.Text;

            if (!String.IsNullOrWhiteSpace(Url))
            {
                string[] res = Url.Trim().TrimEnd('/').Split('/');
                int len = res.Length;
                if (len > 0)
                {
                    if (res[len-1].Equals("releases"))
                    {
                        txtRepoOwner.Text = res[len - 3];
                        txtRepoName.Text = res[len - 2];
                    }
                    else
                    {
                        txtRepoOwner.Text = res[len - 2];
                        txtRepoName.Text = res[len - 1];
                    }
                }
            }
            txtAppVersion.Text = GlobalFunc.GetFileVersion(file);
        }
    }
}
