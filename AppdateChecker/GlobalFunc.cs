using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AppdateChecker
{
    public static class GlobalFunc
    {
        public static void Log(string from, Exception ex)
        {
            Log(from, ex.ToString());
        }
        public static void Log(string from, string log)
        {
            if (String.IsNullOrWhiteSpace(log)) { return; }
            try
            {
                string filepath = Path.Combine(AppContext.BaseDirectory, "appdate.log");
                Console.WriteLine(filepath);
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff")}]: ({from}): {log}");
                }
            }
            catch { Console.WriteLine($"[Debug]: ({from}): {log}"); }
        }
        public static void ShowError(string from, Exception ex, bool showMsg)
        {
            Log(from, ex.ToString());
            if (showMsg && ex!=null)
            {
                MessageBox.Show(ex.ToString(), from);
            }
        }
        public static string SanitizeVersion(string version)
        {
            string ver = version;
            if (!String.IsNullOrWhiteSpace(ver))
            {
                ver = ver.Trim().ToLower().Replace("v", "");
                ver = ver.Replace(" ", "").Replace(',', '.');
                ver = ver.TrimEnd('0').TrimEnd('.');
            }
            return ver;
        }
        public static string GetFileVersion(string file)
        {
            if (!String.IsNullOrWhiteSpace(file))
            {
                try
                {
                    if (File.Exists(file))
                    {
                        var myFileVersionInfo = FileVersionInfo.GetVersionInfo(file);
                        return SanitizeVersion(myFileVersionInfo.FileVersion);
                    }
                }
                catch { }
            }
            return "";
        }
        public static string GetAFile(string Title, string filter, string InitialDir)
        {
            string ret = "";
            OpenFileDialog selectFile = new OpenFileDialog
            {
                InitialDirectory = InitialDir,
                Filter = filter,
                Title = Title,
                CheckFileExists = true,
                CheckPathExists = true,
                RestoreDirectory = true,
                Multiselect = false
            };
            selectFile.ShowDialog();
            if (String.IsNullOrWhiteSpace(selectFile.FileName) == false)
            {
                ret = selectFile.FileName;
            }
            selectFile.Dispose();
            return ret;
        }
    }
}
