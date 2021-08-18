using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppdateChecker.Entity
{
    public class AppEntry
    {
        public AppEntry()
        {
            IsEdited = false;
            Icon = null;
            Uid = 0;
            Name = "";
            RepoOwner = "";
            RepoName = "";
            CurVer = "";
            LatestVer = "";
            Filepath = "";
        }
        public Image Icon { get; set; } = null;
        public ulong Uid { get; set; } = 0;
        public string Name { get; set; } = "";
        public string RepoOwner { get; set; } = "";
        public string RepoName { get; set; } = "";
        public string CurVer { get; set; } = "";
        public string LatestVer { get; set; } = "";
        public string Filepath { get; set; } = "";
        public bool IsEdited { get; set; } = false;
    }
}
