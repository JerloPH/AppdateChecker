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
using AppdateChecker.Entity;

namespace AppdateChecker
{
    public partial class frmMain : Form
    {
        public static class AppCol
        {
            public const int Icon = 0;
            public const int Name = 1;
            public const int RepoOwner = 2;
            public const int RepoName = 3;
            public const int CurVer = 4;
            public const int LatestVer = 5;
            public const int Filepath = 6;
        }
        public static List<DataGridViewRow> UpdatedItems = new List<DataGridViewRow>();

        public frmMain()
        {
            InitializeComponent();
            Logs.Initialize();
        }

        #region Thread-safe functions
        // Add new item, or update existing item
        public void AddToDataGrid(AppEntry data, int Index = -1)
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
                        item.Tag = data.Uid;
                        item.Cells[AppCol.Name].Value = data.Name;
                        item.Cells[AppCol.RepoOwner].Value = data.RepoOwner;
                        item.Cells[AppCol.RepoName].Value = data.RepoName;
                        item.Cells[AppCol.CurVer].Value = data.CurVer;
                        item.Cells[AppCol.LatestVer].Value = data.LatestVer;
                        item.Cells[AppCol.Filepath].Value = data.Filepath;
                    }
                    dgridApps.Refresh();
                }
            }
            catch (Exception ex)
            {
                Logs.Err("AddToDataGrid", ex);
            }
        }
        public void UpdateLatestVer(string newVersion, int index)
        {
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
                Logs.Err("UpdateDataGridViewSource", ex);
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
                    var current = item.Cells[AppCol.CurVer].Value;
                    var latest = item.Cells[AppCol.LatestVer].Value;
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
            timerCheckUpdate.Stop();
            WindowState = FormWindowState.Maximized;
            WindowState = FormWindowState.Normal;
            var formLoad = new frmLoading("Initializing database..", "Loading");
            formLoad.BackgroundWorker.DoWork += (sender1, e1) =>
            {
                SQLHelper.Initiate(); // Initialize db
            };
            formLoad.ShowDialog(this);
            // Load entries
            btnRefresh.PerformClick();
        }
        private void frmMain_Resize(object sender, EventArgs e)
        {
            dgridApps.Height = (int)(ClientRectangle.Height * 0.8);
            btnAdd.Top = dgridApps.Height + 16;
            btnEdit.Top = btnAdd.Top;
            btnSave.Top = btnAdd.Top;
            btnRefresh.Top = btnAdd.Top;
            btnCheckUpdates.Top = btnAdd.Top;
            cbSearchTags.Top = btnAdd.Top;
            cbSearchSkip.Top = cbSearchTags.Bottom + 4;
            cbHideUpdated.Top = btnAdd.Bottom + 4;
            btnVisitPage.Top = cbHideUpdated.Top;
            btnVisitPage.Left = btnRefresh.Left;
        }
        private void dgridApps_Resize(object sender, EventArgs e)
        {
            int W = dgridApps.ClientRectangle.Width;
            double adj = WindowState == FormWindowState.Maximized ? 0.03 : 0;
            dgridApps.Columns[AppCol.Icon].Width = (int)(W * 0.05);
            dgridApps.Columns[AppCol.Name].Width = (int)(W * 0.15);
            dgridApps.Columns[AppCol.RepoOwner].Width = (int)(W * 0.15);
            dgridApps.Columns[AppCol.RepoName].Width = (int)(W * 0.15);
            dgridApps.Columns[AppCol.CurVer].Width = (int)(W * 0.08);
            dgridApps.Columns[AppCol.LatestVer].Width = (int)(W * 0.1);
            dgridApps.Columns[AppCol.Filepath].Width = (int)(W * (0.25 + adj));
        }
        private void btnCheckUpdates_Click(object sender, EventArgs e)
        {
            string callFrom = "frmMain-btnCheckUpdates_Click";
            string repoOwner = "";
            string repoName = "";
            string tagName = "";
            long maxprogress = 0;
            bool skipDev = cbSearchSkip.Checked;
            List<DataGridViewRow> data = new List<DataGridViewRow>();
            var skip = new string[] { "alpha", "beta", "prerelease", "pre-release" };

            if (dgridApps.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow o in dgridApps.SelectedRows)
                {
                    data.Add(o);
                }
            }
            else
            {
                if (dgridApps.Rows.Count > 0)
                {
                    foreach (DataGridViewRow o in dgridApps.Rows)
                    {
                        data.Add(o);
                    }
                }
            }
            maxprogress = data.Count;
            if (maxprogress > 0)
            {
                btnCheckUpdates.Enabled = false;
                var form = new frmLoading("Checking for App Updates..", "Loading", true);
                form.MaxProgress = maxprogress;
                form.BackgroundWorker.DoWork += (sender1, e1) =>
                {
                    foreach (DataGridViewRow item in data)
                    {
                        repoOwner = (item.Cells[2].Value != null ? item.Cells[2].Value.ToString() : "");
                        repoName = (item.Cells[3].Value != null ? item.Cells[3].Value.ToString() : "");
                        if (!String.IsNullOrWhiteSpace(repoOwner) && !String.IsNullOrWhiteSpace(repoName))
                        {
                            Logs.App(callFrom, $"Updating [{item.Cells[1].Value}]");
                            try
                            {
                                var client = new GitHubClient(new ProductHeaderValue("AppdateChecker"));
                                if (cbSearchTags.Checked)
                                {
                                    // Check for Tags
                                    var tags = client.Repository.GetAllTags(repoOwner, repoName).Result;
                                    if (tags?.Count > 0)
                                    {
                                        for (int i=0; i<tags.Count; i++)
                                        {
                                            tagName = tags[i].Name.ToLower();
                                            if (skipDev)
                                            {
                                                if (skip.Any(tagName.Contains))
                                                {
                                                    continue;
                                                }
                                            }
                                            break;
                                        }
                                        tagName = GlobalFunc.SanitizeVersion(tagName);
                                        Logs.App(callFrom, $"Tag [{tags[0].Name}] found! Latest ver: [{tagName}]");
                                        UpdateLatestVer(tagName, item.Index);
                                    }
                                    else
                                    {
                                        Logs.App(callFrom, $"No tags found for [{item.Cells[1].Value}]!");
                                    }
                                }
                                else
                                {
                                    IReadOnlyList<Release> release = client.Repository.Release.GetAll(repoOwner, repoName).Result;
                                    if (release?.Count > 0)
                                    {
                                        for (int i = 0; i < release.Count; i++)
                                        {
                                            try { tagName = release[i].TagName.ToLower(); }
                                            catch { tagName = ""; }

                                            if (skipDev && !String.IsNullOrWhiteSpace(tagName))
                                            {
                                                if (skip.Any(tagName.Contains) || release[i].Prerelease)
                                                {
                                                    continue;
                                                }
                                            }
                                            
                                            tagName = GlobalFunc.SanitizeVersion(tagName);
                                            Logs.App(callFrom, $"Release [{release[i].Name}] found! Latest ver: [{tagName}]");
                                            UpdateLatestVer(tagName, item.Index);
                                            break;
                                        }
                                        Logs.App(callFrom, $"Done updating [{item.Cells[1].Value}]!");
                                    }
                                    else
                                    {
                                        Logs.App(callFrom, $"No releases found for [{item.Cells[1].Value}]!");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logs.Err(callFrom, ex);
                            }
                        }
                        form.UpdateProgress();
                        System.Threading.Thread.Sleep(500);
                    }
                };
                form.ShowDialog(this);

                foreach (var item in data)
                {
                    UpdatedItems.Add(item);
                }
            }
            data.Clear();
            btnSave.PerformClick();
            timerCheckUpdate.Interval = 15000;
            timerCheckUpdate.Start();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var form = new frmAppEntry(null);
            form.ShowDialog(this);
            var Result = form.Result;
            if (Result != null)
            {
                Result.Uid = 0;
                AddToDataGrid(Result);
            }
            form?.Dispose();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string callFrom = "frmMain-btnSave_Click";
            long maxprogress = UpdatedItems.Count;
            var data = new List<AppEntry>();

            if (maxprogress > 0)
            {
                foreach (var item in UpdatedItems)
                {
                    if (ulong.TryParse(item.Tag.ToString(), out ulong uid))
                    {
                        var entry = new AppEntry();
                        entry.Uid = uid;
                        entry.Name = item.Cells[AppCol.Name].Value?.ToString();
                        entry.RepoOwner = item.Cells[AppCol.RepoOwner].Value?.ToString();
                        entry.RepoName = item.Cells[AppCol.RepoName].Value?.ToString();
                        entry.CurVer = item.Cells[AppCol.CurVer].Value?.ToString();
                        entry.LatestVer = item.Cells[AppCol.LatestVer].Value?.ToString();
                        entry.Filepath = item.Cells[AppCol.Filepath].Value?.ToString();
                        data.Add(entry);
                        Logs.Debug($"Updated item ({entry.Uid}): {entry.Name}");
                    }
                }
                UpdatedItems.Clear();
            }
            else
            {
                foreach (DataGridViewRow item in dgridApps.Rows)
                {
                    if (ulong.TryParse(item.Tag.ToString(), out ulong uid))
                    {
                        var entry = new AppEntry();
                        entry.Uid = uid;
                        entry.Name = item.Cells[AppCol.Name].Value?.ToString();
                        entry.RepoOwner = item.Cells[AppCol.RepoOwner].Value?.ToString();
                        entry.RepoName = item.Cells[AppCol.RepoName].Value?.ToString();
                        entry.CurVer = item.Cells[AppCol.CurVer].Value?.ToString();
                        entry.LatestVer = item.Cells[AppCol.LatestVer].Value?.ToString();
                        entry.Filepath = item.Cells[AppCol.Filepath].Value?.ToString();
                        data.Add(entry);
                    }
                }
            }
            maxprogress = data.Count;
            if (maxprogress > 0)
            {
                Logs.App(callFrom, $"Start saving entries..");
                var form = new frmLoading("Saving to database..", "Loading", true);
                form.MaxProgress = maxprogress;
                Logs.App(callFrom, $"Max Progress: [{maxprogress}]");
                form.BackgroundWorker.DoWork += (sender1, e1) =>
                {
                    var dtInfo = new Dictionary<string, string>();
                    var dtFile = new Dictionary<string, string>();
                    foreach (var item in data)
                    {
                        dtInfo?.Clear();
                        dtFile?.Clear();
                        // Info
                        dtInfo.Add(SQLHelper.DbColId, item.Uid.ToString());
                        dtInfo.Add(SQLHelper.DbColName, item.Name);
                        dtInfo.Add(SQLHelper.DbColRepoOwner, item.RepoOwner);
                        dtInfo.Add(SQLHelper.DbColRepoName, item.RepoName);
                        dtInfo.Add(SQLHelper.DbColCurVer, item.CurVer);
                        dtInfo.Add(SQLHelper.DbColLatestVer, item.LatestVer);
                        // Paths
                        dtFile.Add(SQLHelper.DbColId, item.Uid.ToString());
                        dtFile.Add(SQLHelper.DbColFilepath, item.Filepath);

                        Logs.App(callFrom, $"Uid [{item.Uid}], Name: {item.Name}");
                        if (item.Uid == 0) // New entry
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
                        form.UpdateProgress();
                    }
                };
                form.ShowDialog(this);
                btnRefresh.PerformClick();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            // Reload entries from database
            string callFrom = "frmMain-btnRefresh_Click";
            long maxprogress = 0;
            string filepath = "";
            string version = "";
            string newfileversion = "";
            string UIDS = "";
            string query = $"SELECT * FROM {SQLHelper.DbTableApp} A LEFT JOIN {SQLHelper.DbTableAppPath} B ON A.`{SQLHelper.DbColId}`=B.`{SQLHelper.DbColId}`";

            if (dgridApps.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow item in dgridApps.SelectedRows)
                {
                    if (!UpdatedItems.Contains(item))
                        UpdatedItems.Add(item);
                }
            }

            if (UpdatedItems.Count > 0)
            {
                foreach (var item in UpdatedItems)
                {
                    UIDS += $"{item.Tag.ToString()},";
                    try
                    {
                        dgridApps.Rows.Remove(item);
                    }
                    catch (Exception ex)
                    {
                        GlobalFunc.ShowError(callFrom, ex, true);
                    }
                }
                UIDS = UIDS.TrimEnd(',');
                UpdatedItems.Clear();
                query += $" WHERE A.`{SQLHelper.DbColId}` IN ({UIDS})";
            }
            else
            {
                dgridApps.Rows.Clear();
            }

            var form = new frmLoading("Refreshing entries..", "Loading", true);
            form.BackgroundWorker.DoWork += (sender1, e1) =>
            {
                var dt = SQLHelper.Query(query, callFrom);
                if (dt.Rows?.Count > 0)
                {
                    Logs.Debug();
                    maxprogress = dt.Rows.Count;
                    form.MaxProgress = maxprogress;
                    foreach (DataRow row in dt.Rows)
                    {
                        if (ulong.TryParse(row[SQLHelper.DbColId]?.ToString(), out ulong uid))
                        {
                            var data = new AppEntry();
                            Logs.Debug($"App Name: {row[SQLHelper.DbColName]}");
                            filepath = row[SQLHelper.DbColFilepath]?.ToString();
                            version = row[SQLHelper.DbColCurVer]?.ToString();
                            newfileversion = GlobalFunc.GetFileVersion(filepath);
                            Logs.Debug($"Old FileVersion: {version}, New FileVersion: {newfileversion}\n");

                            data.Uid = uid;
                            data.Name = row[SQLHelper.DbColName]?.ToString();
                            data.RepoOwner = row[SQLHelper.DbColRepoOwner]?.ToString();
                            data.RepoName = row[SQLHelper.DbColRepoName]?.ToString();
                            data.CurVer = (String.IsNullOrWhiteSpace(newfileversion) ? version : newfileversion);
                            data.LatestVer = row[SQLHelper.DbColLatestVer]?.ToString();
                            data.Filepath = filepath;

                            AddToDataGrid(data);
                        }
                        form.UpdateProgress();
                    }
                }
            };
            form.ShowDialog(this);
            dgridApps.Sort(dgridApps.Columns[AppCol.Name], ListSortDirection.Ascending);
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

        private void timerCheckUpdate_Tick(object sender, EventArgs e)
        {
            if (btnCheckUpdates.Enabled == false)
                btnCheckUpdates.Enabled = true;

            timerCheckUpdate.Stop();
        }

        private void btnVisitPage_Click(object sender, EventArgs e)
        {
            if (dgridApps.SelectedRows.Count > 0)
            {
                if (dgridApps.SelectedRows.Count > 1)
                {
                    GlobalFunc.ShowWarning("Select only 1 Row!");
                    return;
                }
                try
                {
                    var row = dgridApps.SelectedRows[0];
                    string repoOwner = row.Cells[AppCol.RepoOwner].Value.ToString();
                    string repoName = row.Cells[AppCol.RepoName].Value.ToString();
                    System.Diagnostics.Process.Start("explorer.exe", $"https://github.com/{repoOwner}/{repoName}");
                }
                catch (Exception ex)
                {
                    GlobalFunc.ShowError("frmMain-btnVisitPage_Click", ex, true);
                }
            }
            else
            {
                GlobalFunc.ShowWarning("Select A Row!");
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            int count = dgridApps.SelectedRows.Count;
            if (count > 0)
            {
                if (count == 1)
                {
                    var item = dgridApps.SelectedRows[0];
                    if (ulong.TryParse(item.Tag.ToString(), out ulong uid))
                    {
                        var entry = new AppEntry();
                        entry.Uid = uid;
                        entry.Name = item.Cells[AppCol.Name].Value?.ToString();
                        entry.RepoOwner = item.Cells[AppCol.RepoOwner].Value?.ToString();
                        entry.RepoName = item.Cells[AppCol.RepoName].Value?.ToString();
                        entry.CurVer = item.Cells[AppCol.CurVer].Value?.ToString();
                        entry.LatestVer = item.Cells[AppCol.LatestVer].Value?.ToString();
                        entry.Filepath = item.Cells[AppCol.Filepath].Value?.ToString();
                        var form = new frmAppEntry(entry);
                        form.ShowDialog(this);
                        AddToDataGrid(form.Result, item.Index);
                        form?.Dispose();
                        if (form.Result.IsEdited)
                        {
                            UpdatedItems.Add(item);
                            form.Result.IsEdited = false;
                        }
                    }
                }
                else
                    GlobalFunc.ShowWarning("Only 1 entry can be edited at a time!");
            }
            else
                GlobalFunc.ShowWarning("Select an entry to edit!");
        }
    }
}
