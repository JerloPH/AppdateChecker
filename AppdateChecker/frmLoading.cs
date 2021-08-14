using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AppdateChecker
{
    public partial class frmLoading : Form
    {
        public string Caption
        {
            get { return this.Text; }
            set { this.Text = value; }
        }
        public string Message
        {
            get { return label1.Text; }
            set
            {
                if (label1.InvokeRequired)
                {
                    BeginInvoke((Action)delegate
                    {
                        label1.Text = value;
                    });
                }
                else
                    label1.Text = value;
            }
        }
        private long MaxProgressHidden = 0;
        public long MaxProgress
        {
            get { return MaxProgressHidden; }
            set { MaxProgressHidden = value; }
        }
        private long ProgressCountHidden = 0;
        public long ProgressCount
        {
            get { return ProgressCountHidden; }
            set { ProgressCountHidden = value; }
        }

        public frmLoading()
        {
            InitializeComponent();
        }
        public frmLoading(string message, string caption, bool useProgress = false)
        {
            InitializeComponent();
            Message = message;
            Caption = (!String.IsNullOrWhiteSpace(caption) ? caption : "Appdate Checker");
            CenterToParent();
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.BringToFront();
            if (useProgress)
            {
                this.BackgroundWorker.WorkerReportsProgress = true;
                this.BackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(this.BackgroundWorker_ProgressChanged);
            }
            lblProgress.Visible = useProgress;
        }

        #region Thread-safe functions
        private delegate void UpdateMessageThreadSafeDelegate(string message);
        public void UpdateMessage(string message)
        {
            if (label1.InvokeRequired)
            {
                label1.Invoke(new UpdateMessageThreadSafeDelegate(UpdateMessage), new object[] { message });
            }
            else
            {
                label1.Text = message;
            }
        }
        #endregion
        #region Public Functions
        public void UpdateProgress(long progress)
        {
            BackgroundWorker.ReportProgress((int)((progress / MaxProgress) * 100), progress);
        }
        #endregion

        private void frmPopulateMovie_Shown(object sender, EventArgs e)
        {
            if (BackgroundWorker.IsBusy)
                return;
            lblProgress.Text = $"Progress: {ProgressCount} / {MaxProgress}";
            BackgroundWorker.RunWorkerAsync();
        }

        private void frmPopulateMovie_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (BackgroundWorker.IsBusy)
                e.Cancel = true;

            pictureBox1.Image?.Dispose();
            Dispose();
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Close();
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage > -1 && MaxProgress > 0)
            {
                try
                {
                    long value = Convert.ToInt64(e.UserState);
                    if (value > 0)
                    {
                        ProgressCount = value;
                    }
                    else
                    {
                        ProgressCount += 1;
                    }
                }
                catch { ProgressCount += 1; }
            }
            lblProgress.Text = $"Progress: {ProgressCount} / {MaxProgress}";
        }
        
        private void frmLoading_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }
    }
}
