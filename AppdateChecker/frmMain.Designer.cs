
namespace AppdateChecker
{
    partial class frmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnCheckUpdates = new System.Windows.Forms.Button();
            this.dgridApps = new System.Windows.Forms.DataGridView();
            this.cImg = new System.Windows.Forms.DataGridViewImageColumn();
            this.cApp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cRepoOwner = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cRepoName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cCurVer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cLatestVer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFilePath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.cbHideUpdated = new System.Windows.Forms.CheckBox();
            this.timerCheckUpdate = new System.Windows.Forms.Timer(this.components);
            this.cbSearchTags = new System.Windows.Forms.CheckBox();
            this.cbSearchSkip = new System.Windows.Forms.CheckBox();
            this.btnVisitPage = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgridApps)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCheckUpdates
            // 
            this.btnCheckUpdates.Location = new System.Drawing.Point(522, 411);
            this.btnCheckUpdates.Name = "btnCheckUpdates";
            this.btnCheckUpdates.Size = new System.Drawing.Size(159, 50);
            this.btnCheckUpdates.TabIndex = 2;
            this.btnCheckUpdates.Text = "Check Updates";
            this.btnCheckUpdates.UseVisualStyleBackColor = true;
            this.btnCheckUpdates.Click += new System.EventHandler(this.btnCheckUpdates_Click);
            // 
            // dgridApps
            // 
            this.dgridApps.AllowUserToAddRows = false;
            this.dgridApps.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgridApps.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.cImg,
            this.cApp,
            this.cRepoOwner,
            this.cRepoName,
            this.cCurVer,
            this.cLatestVer,
            this.colFilePath});
            this.dgridApps.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgridApps.Location = new System.Drawing.Point(0, 0);
            this.dgridApps.Name = "dgridApps";
            this.dgridApps.RowHeadersWidth = 51;
            this.dgridApps.RowTemplate.Height = 29;
            this.dgridApps.Size = new System.Drawing.Size(1034, 405);
            this.dgridApps.TabIndex = 4;
            this.dgridApps.UserDeletedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.dgridApps_UserDeletedRow);
            this.dgridApps.Resize += new System.EventHandler(this.dgridApps_Resize);
            // 
            // cImg
            // 
            this.cImg.HeaderText = "Icon";
            this.cImg.MinimumWidth = 6;
            this.cImg.Name = "cImg";
            this.cImg.ReadOnly = true;
            this.cImg.Width = 60;
            // 
            // cApp
            // 
            this.cApp.HeaderText = "App Name";
            this.cApp.MinimumWidth = 6;
            this.cApp.Name = "cApp";
            this.cApp.Width = 140;
            // 
            // cRepoOwner
            // 
            this.cRepoOwner.HeaderText = "Repo Owner";
            this.cRepoOwner.MinimumWidth = 6;
            this.cRepoOwner.Name = "cRepoOwner";
            this.cRepoOwner.Width = 125;
            // 
            // cRepoName
            // 
            this.cRepoName.HeaderText = "Repo Name";
            this.cRepoName.MinimumWidth = 6;
            this.cRepoName.Name = "cRepoName";
            this.cRepoName.Width = 125;
            // 
            // cCurVer
            // 
            this.cCurVer.HeaderText = "Current Version";
            this.cCurVer.MinimumWidth = 6;
            this.cCurVer.Name = "cCurVer";
            this.cCurVer.Width = 125;
            // 
            // cLatestVer
            // 
            this.cLatestVer.HeaderText = "Latest Version";
            this.cLatestVer.MinimumWidth = 6;
            this.cLatestVer.Name = "cLatestVer";
            this.cLatestVer.ReadOnly = true;
            this.cLatestVer.Width = 125;
            // 
            // colFilePath
            // 
            this.colFilePath.HeaderText = "File Path";
            this.colFilePath.MinimumWidth = 6;
            this.colFilePath.Name = "colFilePath";
            this.colFilePath.Width = 280;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(12, 411);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(111, 50);
            this.btnAdd.TabIndex = 5;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(248, 411);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(131, 50);
            this.btnSave.TabIndex = 6;
            this.btnSave.Text = "Save to DB";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(385, 411);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(131, 50);
            this.btnRefresh.TabIndex = 7;
            this.btnRefresh.Text = "Reload Entries";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // cbHideUpdated
            // 
            this.cbHideUpdated.AutoSize = true;
            this.cbHideUpdated.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cbHideUpdated.Location = new System.Drawing.Point(12, 467);
            this.cbHideUpdated.Name = "cbHideUpdated";
            this.cbHideUpdated.Size = new System.Drawing.Size(207, 32);
            this.cbHideUpdated.TabIndex = 8;
            this.cbHideUpdated.Text = "Hide Updated Apps";
            this.cbHideUpdated.UseVisualStyleBackColor = true;
            this.cbHideUpdated.CheckedChanged += new System.EventHandler(this.cbHideUpdated_CheckedChanged);
            // 
            // timerCheckUpdate
            // 
            this.timerCheckUpdate.Interval = 10000;
            this.timerCheckUpdate.Tick += new System.EventHandler(this.timerCheckUpdate_Tick);
            // 
            // cbSearchTags
            // 
            this.cbSearchTags.AutoSize = true;
            this.cbSearchTags.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cbSearchTags.Location = new System.Drawing.Point(687, 411);
            this.cbSearchTags.Name = "cbSearchTags";
            this.cbSearchTags.Size = new System.Drawing.Size(325, 32);
            this.cbSearchTags.TabIndex = 9;
            this.cbSearchTags.Text = "Search for Tags, instead of release";
            this.cbSearchTags.UseVisualStyleBackColor = true;
            // 
            // cbSearchSkip
            // 
            this.cbSearchSkip.AutoSize = true;
            this.cbSearchSkip.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cbSearchSkip.Location = new System.Drawing.Point(687, 440);
            this.cbSearchSkip.Name = "cbSearchSkip";
            this.cbSearchSkip.Size = new System.Drawing.Size(317, 32);
            this.cbSearchSkip.TabIndex = 10;
            this.cbSearchSkip.Text = "Skip pre-release, alpha, and beta";
            this.cbSearchSkip.UseVisualStyleBackColor = true;
            // 
            // btnVisitPage
            // 
            this.btnVisitPage.Location = new System.Drawing.Point(286, 467);
            this.btnVisitPage.Name = "btnVisitPage";
            this.btnVisitPage.Size = new System.Drawing.Size(148, 50);
            this.btnVisitPage.TabIndex = 11;
            this.btnVisitPage.Text = "Visit Project Page";
            this.btnVisitPage.UseVisualStyleBackColor = true;
            this.btnVisitPage.Click += new System.EventHandler(this.btnVisitPage_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.Location = new System.Drawing.Point(129, 411);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(111, 50);
            this.btnEdit.TabIndex = 12;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1034, 633);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnVisitPage);
            this.Controls.Add(this.cbSearchSkip);
            this.Controls.Add(this.cbSearchTags);
            this.Controls.Add(this.cbHideUpdated);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.dgridApps);
            this.Controls.Add(this.btnCheckUpdates);
            this.MinimumSize = new System.Drawing.Size(1052, 680);
            this.Name = "frmMain";
            this.Text = "Appdate Checler";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.Resize += new System.EventHandler(this.frmMain_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.dgridApps)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnCheckUpdates;
        private System.Windows.Forms.DataGridView dgridApps;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.CheckBox cbHideUpdated;
        private System.Windows.Forms.DataGridViewImageColumn cImg;
        private System.Windows.Forms.DataGridViewTextBoxColumn cApp;
        private System.Windows.Forms.DataGridViewTextBoxColumn cRepoOwner;
        private System.Windows.Forms.DataGridViewTextBoxColumn cRepoName;
        private System.Windows.Forms.DataGridViewTextBoxColumn cCurVer;
        private System.Windows.Forms.DataGridViewTextBoxColumn cLatestVer;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFilePath;
        private System.Windows.Forms.Timer timerCheckUpdate;
        private System.Windows.Forms.CheckBox cbSearchTags;
        private System.Windows.Forms.CheckBox cbSearchSkip;
        private System.Windows.Forms.Button btnVisitPage;
        private System.Windows.Forms.Button btnEdit;
    }
}

