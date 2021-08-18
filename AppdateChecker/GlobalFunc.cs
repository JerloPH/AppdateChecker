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
    public static class Logs
    {
        private static string FileAppLog = "";
        private static string FileErrlog = "";
        private static string Debuglog = "";

        public static string FileDblog = "";

        public static bool Initialize()
        {
            try
            {
                FileAppLog = Path.Combine(AppContext.BaseDirectory, "appdate.log");
                FileErrlog = Path.Combine(AppContext.BaseDirectory, "appdate_error.log"); ;
                FileDblog = Path.Combine(AppContext.BaseDirectory, "appdate_db.log");
                Debuglog = Path.Combine(AppContext.BaseDirectory, "appdate_debug.log");
                return true;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return false;
        }
        public static void Log(string filepath, string from, string log)
        {
            if (String.IsNullOrWhiteSpace(log)) { return; }
            try
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff")}]: ({from}): {log}");
                }
            }
            catch { Console.WriteLine($"[Debug]: ({from}): {log}"); }
        }
        public static void Debug(string log)
        {
            Log(Debuglog, "", log);
        }
        public static void Debug()
        {
            Log(Debuglog, "", "\n####################################################################\n");
        }
        public static void App(string from, string log)
        {
            Log(FileAppLog, from, log);
        }
        public static void Err(string from, Exception ex)
        {
            Log(FileErrlog, from, ex.ToString());
        }
        
    }
    public static class GlobalFunc
    {
        public static void ShowMessage(string message, string caption = "Appdate Checker")
        {
            MessageBox.Show(message, caption);
        }
        public static void ShowWarning(string message, string caption = "Appdate Checker")
        {
            ShowMessage(message, caption);
        }
        public static void ShowError(string from, Exception ex, bool showMsg)
        {
            Logs.Err(from, ex);
            if (showMsg && ex!=null)
            {
               ShowMessage(ex.ToString(), from);
            }
        }
        public static string SanitizeVersion(string version)
        {
            if (String.IsNullOrWhiteSpace(version))
                return "";

            string ver = version;
            try
            {
                ver = ver.Trim().ToLower().Replace("v", "");//System.Text.RegularExpressions.Regex.Replace(ver, "^[0-9.-]+$", "");
                ver = ver.Trim('-').Replace(" ", "").Replace(',', '.');
                Logs.Debug($"Sanitized FileVersion: {ver}");
            }
            catch (Exception ex)
            {
                ver = "";
                Logs.Err("GlobalFunc-SanitizeVersion", ex);
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
                        Logs.Debug($"Raw FileVersion: {myFileVersionInfo.FileVersion}");
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
