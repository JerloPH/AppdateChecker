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
using AppdateChecker.Entity;

namespace AppdateChecker
{
    public partial class frmAppEntry : Form
    {
        public AppEntry Result { get; set; } = new AppEntry();
        private ulong Uid = 0;

        public frmAppEntry(AppEntry Entry)
        {
            InitializeComponent();
            if (Entry == null)
            {
                Text = "Add New App entry";
                Result.Uid = 0;
            }
            else
            {
                Result = Entry;
                Text = "Edit App Entry";
                Uid = Result.Uid;
                txtName.Text = Result.Name;
                txtRepoOwner.Text = Result.RepoOwner;
                txtRepoName.Text = Result.RepoName;
                txtAppVersion.Text = Result.CurVer;
                txtPath.Text = Result.Filepath;
                txtUrl.Text = $"https://github.com/{Result.RepoOwner}/{Result.RepoName}";
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
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

            Result.Uid = this.Uid;
            Result.Name = txtName.Text;
            Result.RepoOwner = txtRepoOwner.Text;
            Result.RepoName = txtRepoName.Text;
            Result.CurVer = txtAppVersion.Text;
            Result.Filepath = txtPath.Text;
            Result.IsEdited = true;
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
