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
using Newtonsoft.Json;
using Octokit;
using Octokit.Helpers;

namespace AppdateChecker
{
    public partial class frmMain : Form
    {
        public static class GridCol
        {
            public const int Name = 1;
            public const int RepoOwner = 2;
            public const int RepoName = 3;
            public const int CurVer = 4;
            public const int LatestVer = 5;
            public const int Filepath = 6;
        }

        public frmMain()
        {
            InitializeComponent();
        }

        #region Thread-safe functions
        // Add new item, or update existing item
        public void AddToDataGrid(Dictionary<string, string> data, int Index = -1)
        {
            try
            {
                int index = Index;
                if (this.dgridApps.InvokeRequired)
                {
                    this.dgridApps.Invoke(new Action(() => AddToDataGrid(data, Index)));
                }
                else
                {
                    if (Index == -1)
                    {
                        index = dgridApps.Rows.Add();
                    }
                    if (index > -1)
                    {
                        var item = dgridApps.Rows[index];
                        item.Tag = data[SQLHelper.DbColId];
                        item.Cells[GridCol.Name].Value = data[SQLHelper.DbColName];
                        item.Cells[GridCol.RepoOwner].Value = data[SQLHelper.DbColRepoOwner];
                        item.Cells[GridCol.RepoName].Value = data[SQLHelper.DbColRepoName];
                        item.Cells[GridCol.CurVer].Value = data[SQLHelper.DbColCurVer];
                        item.Cells[GridCol.LatestVer].Value = data[SQLHelper.DbColLatestVer];
                        item.Cells[GridCol.Filepath].Value = data[SQLHelper.DbColFilepath];
                    }
                    dgridApps.Refresh();
                }
            }
            catch (Exception ex)
            {
                GlobalFunc.Log("UpdateDataGridViewSource", ex);
            }
        }
        public void UpdateLatestVer(string ver, int index)
        {
            string newVersion = GlobalFunc.SanitizeVersion(ver);
            try
            {
                if (this.dgridApps.InvokeRequired)
                {
                    this.dgridApps.Invoke(new Action(() => UpdateLatestVer(newVersion, index)));
                }
                else
                {
                    if (index > -1)
                    {
                        dgridApps.Rows[index].Cells[5].Value = newVersion;
                    }
                    dgridApps.Refresh();
                }
            }
            catch (Exception ex)
            {
                GlobalFunc.Log("UpdateDataGridViewSource", ex);
            }
        }
        public void HideUpdated()
        {
            bool hide = cbHideUpdated.Checked;
            foreach (DataGridViewRow item in dgridApps.Rows)
            {
                if (!hide)
                {
                    item.Visible = true;
                }
                else
                {
                    // Match current ver to latest ver
                    var current = item.Cells[GridCol.CurVer].Value;
                    var latest = item.Cells[GridCol.LatestVer].Value;
                    if (current!=null && latest!=null)
                    {
                        if (current.ToString().Equals(latest.ToString()))
                        {
                            item.Visible = false;
                        }
                    }
                }
            }
        }
        #endregion

        private void frmMain_Load(object sender, EventArgs e)
        {
            var formLoad = new frmLoading("Initializing database..", "Loading");
            formLoad.BackgroundWorker.DoWork += (sender1, e1) =>
            {
                SQLHelper.Initiate(); // Initialize db
            };
            formLoad.ShowDialog(this);
            // Load entries
            btnRefresh.PerformClick();
        }
        private void btnCheckUpdates_Click(object sender, EventArgs e)
        {
            string callFrom = "frmMain-btnCheckUpdates_Click";
            string repoOwner = "";
            string repoName = "";
            string tagName = "";
            long maxprogress = dgridApps.Rows.Count;
            long progress = 0;
            if (maxprogress > 0)
            {
                var form = new frmLoading("Checking for App Updates..", "Loading", true);
                form.MaxProgress = maxprogress;
                form.BackgroundWorker.DoWork += (sender1, e1) =>
                {
                    foreach (DataGridViewRow item in dgridApps.Rows)
                    {
                        progress += 1;
                        repoOwner = (item.Cells[2].Value != null ? item.Cells[2].Value.ToString() : "");
                        repoName = (item.Cells[3].Value != null ? item.Cells[3].Value.ToString() : "");
                        if (!String.IsNullOrWhiteSpace(repoOwner) && !String.IsNullOrWhiteSpace(repoName))
                        {
                            GlobalFunc.Log(callFrom, $"Updating [{item.Cells[1].Value}]");
                            try
                            {
                                var client = new GitHubClient(new ProductHeaderValue("AppdateChecker"));
                                IReadOnlyList<Release> release = client.Repository.Release.GetAll(repoOwner, repoName).Result;
                                if (release?.Count > 0)
                                {
                                    for (int i = 0; i < release.Count; i++)
                                    {
                                        if (release[i].Prerelease)
                                            continue;

                                        tagName = release[i].TagName;
                                        tagName = tagName.Trim().ToLower().Replace("v", "");
                                        GlobalFunc.Log(callFrom, $"Release [{release[i].Name}] found! Latest ver: [{tagName}]");
                                        UpdateLatestVer(tagName, item.Index);
                                        break;
                                    }
                                    GlobalFunc.Log(callFrom, $"Done updating [{item.Cells[1].Value}]!");
                                }
                                else
                                {
                                    GlobalFunc.Log(callFrom, $"No releases found for [{item.Cells[1].Value}]!");
                                }
                            }
                            catch (Exception ex)
                            {
                                GlobalFunc.Log(callFrom, ex);
                            }
                        }
                        form.UpdateProgress(progress);
                    }
                };
                form.ShowDialog(this);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var form = new frmAppEntry(null);
            form.ShowDialog(this);
            var Result = form.Result;
            if (Result != null)
            {
                Result.Add(SQLHelper.DbColLatestVer, "0");
                AddToDataGrid(Result);
            }
            form.Dispose();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string callFrom = "frmMain-btnSave_Click";
            long maxprogress = dgridApps.Rows.Count;
            long progress = 0;

            if (maxprogress > 0)
            {
                GlobalFunc.Log(callFrom, $"Start saving entries..");
                
                var form = new frmLoading("Saving to database..", "Loading", true);
                form.MaxProgress = maxprogress;
                GlobalFunc.Log(callFrom, $"Max Progress: [{maxprogress}]");
                form.BackgroundWorker.DoWork += (sender1, e1) =>
                {
                    var dtInfo = new Dictionary<string, string>();
                    var dtFile = new Dictionary<string, string>();
                    foreach (DataGridViewRow item in dgridApps.Rows)
                    {
                        progress += 1;
                        dtInfo?.Clear();
                        dtFile?.Clear();
                        if (item.Tag != null)
                        {
                            // Info
                            dtInfo.Add(SQLHelper.DbColId, item.Tag.ToString());
                            dtInfo.Add(SQLHelper.DbColName, item.Cells[1].Value?.ToString());
                            dtInfo.Add(SQLHelper.DbColRepoOwner, item.Cells[2].Value?.ToString());
                            dtInfo.Add(SQLHelper.DbColRepoName, item.Cells[3].Value?.ToString());
                            dtInfo.Add(SQLHelper.DbColCurVer, item.Cells[4].Value?.ToString());
                            dtInfo.Add(SQLHelper.DbColLatestVer, item.Cells[5].Value?.ToString());
                            // Paths
                            dtFile.Add(SQLHelper.DbColId, item.Tag.ToString());
                            dtFile.Add(SQLHelper.DbColFilepath, item.Cells[6].Value?.ToString());

                            GlobalFunc.Log(callFrom, $"Uid [{item.Tag}], Name: {item.Cells[1].Value?.ToString()}");
                            if (item.Tag.ToString().Equals("0")) // New entry
                            {
                                SQLHelper.InsertNewApp(dtInfo, dtFile, callFrom);
                            }
                            else
                            {
                                if (SQLHelper.UpdateTable(SQLHelper.DbTableApp, dtInfo, callFrom))
                                {
                                    SQLHelper.UpdateTable(SQLHelper.DbTableAppPath, dtFile, callFrom);
                                }
                            }
                        }
                        form.UpdateProgress(progress);
                    }
                };
                form.ShowDialog(this);
                btnRefresh.PerformClick();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            // Reload entries from database
            dgridApps.Rows.Clear();
            string callFrom = "frmMain-btnRefresh_Click";
            long progress = 0;
            long maxprogress = 0;
            string filepath = "";
            string version = "";
            string newfileversion = "";
            var form = new frmLoading("Refreshing entries..", "Loading", true);
            form.BackgroundWorker.DoWork += (sender1, e1) =>
            {
                var dt = SQLHelper.Query($"SELECT * FROM {SQLHelper.DbTableApp} A LEFT JOIN {SQLHelper.DbTableAppPath} B ON A.`{SQLHelper.DbColId}`=B.`{SQLHelper.DbColId}`", callFrom);
                if (dt.Rows?.Count > 0)
                {
                    maxprogress = dt.Rows.Count;
                    form.MaxProgress = maxprogress;
                    foreach (DataRow row in dt.Rows)
                    {
                        progress += 1;
                        var data = new Dictionary<string, string>();
                        filepath = row[SQLHelper.DbColFilepath]?.ToString();
                        version = row[SQLHelper.DbColCurVer]?.ToString();
                        newfileversion = GlobalFunc.GetFileVersion(filepath);

                        data.Add(SQLHelper.DbColId, row[SQLHelper.DbColId]?.ToString());
                        data.Add(SQLHelper.DbColName, row[SQLHelper.DbColName]?.ToString());
                        data.Add(SQLHelper.DbColRepoOwner, row[SQLHelper.DbColRepoOwner]?.ToString());
                        data.Add(SQLHelper.DbColRepoName, row[SQLHelper.DbColRepoName]?.ToString());
                        data.Add(SQLHelper.DbColCurVer, (!String.IsNullOrWhiteSpace(newfileversion) ? newfileversion : version));
                        data.Add(SQLHelper.DbColLatestVer, row[SQLHelper.DbColLatestVer]?.ToString());
                        data.Add(SQLHelper.DbColFilepath, filepath);
                        
                        AddToDataGrid(data);
                        data.Clear();
                        form.UpdateProgress(progress);
                    }
                }
            };
            form.ShowDialog(this);
            dgridApps.Sort(dgridApps.Columns[1], ListSortDirection.Ascending);
            HideUpdated();
        }

        private void cbHideUpdated_CheckedChanged(object sender, EventArgs e)
        {
            HideUpdated();
        }

        private void dgridApps_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            if (e.Row != null)
            {
                try
                {
                    string uid = e.Row.Tag.ToString();
                    var form = new frmLoading("Deleting entry", "");
                    form.BackgroundWorker.DoWork += (sender1, e1) =>
                    {
                        SQLHelper.DeleteItem(uid);
                    };
                    form.ShowDialog(this);
                }
                catch { }
            }
        }
    }
}
