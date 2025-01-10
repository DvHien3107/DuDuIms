using RunAtTime.Const;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunAtTime.Module
{
    public static class LogModule
    {
        private static string logRequset = "";
        private static string logRespon = "";
        private static string logError = "";
        private static string logInfo = "";


        public static void AddLogRequest(string newLog)
        {
            try
            {
                logRequset += newLog + Environment.NewLine;
            }
            catch { }
        }
        public static void AddLogRespon(string newLog)
        {
            try
            {
                logRespon += newLog + Environment.NewLine;
            }
            catch { }
        }
        public static void AddLogError(string newLog)
        {
            try
            {
                logError += newLog + Environment.NewLine;
            }
            catch { }
        }
        public static void AddInfo(string newLog)
        {
            try
            {
                logInfo += newLog + Environment.NewLine;
            }
            catch { }
        }
        public static void flushLog()
        {
            var timer = Interval.Set(() =>
            {
                string today = DateTime.UtcNow.ToString("MMddyyyy");
                string folderUpdate = Constant.staticPathFile + $"\\Requset_{today}.txt";
                try
                {
                    if (!File.Exists(folderUpdate))
                    {
                        using (FileStream fs = File.Create(folderUpdate))
                        {
                            // Add some text to file    
                            Byte[] title = new UTF8Encoding(true).GetBytes(logRequset);
                            fs.Write(title, 0, title.Length);
                        }
                    }
                    else
                    {
                        File.AppendAllText(folderUpdate, logRequset);
                        logRequset = "";
                    }
                }
                catch { }
                folderUpdate = Constant.staticPathFile + $"\\Response_{today}.txt";
                try
                {
                    if (!File.Exists(folderUpdate))
                    {
                        using (FileStream fs = File.Create(folderUpdate))
                        {
                            // Add some text to file    
                            Byte[] title = new UTF8Encoding(true).GetBytes(logRespon);
                            fs.Write(title, 0, title.Length);
                        }
                    }
                    else
                    {
                        File.AppendAllText(folderUpdate, logRespon);
                        logRespon = "";
                    }
                }
                catch { }
                folderUpdate = Constant.staticPathFile + $"\\Error_{today}.txt";
                try
                {
                    if (!File.Exists(folderUpdate))
                    {
                        using (FileStream fs = File.Create(folderUpdate))
                        {
                            // Add some text to file    
                            Byte[] title = new UTF8Encoding(true).GetBytes(logError);
                            fs.Write(title, 0, title.Length);
                        }
                    }
                    else
                    {
                        File.AppendAllText(folderUpdate, logError);
                        logError = "";
                    }
                }
                catch { }
                folderUpdate = Constant.staticPathFile + $"\\Info_{today}.txt";
                try
                {
                    if (!File.Exists(folderUpdate))
                    {
                        using (FileStream fs = File.Create(folderUpdate))
                        {
                            // Add some text to file    
                            Byte[] title = new UTF8Encoding(true).GetBytes(logInfo);
                            fs.Write(title, 0, title.Length);
                        }
                    }
                    else
                    {
                        File.AppendAllText(folderUpdate, logInfo);
                        logInfo = "";
                    }
                }
                catch { }
            }, 60000);
        }
        public static void flushDay()
        {
            clearOldFile();
            var timer = Interval.Set(() =>
            {
                clearOldFile();
            }, 86400000);

        }
        private static void clearOldFile()
        {
            string[] files = Directory.GetFiles(Constant.staticPathFile);

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.LastAccessTime < DateTime.Now.AddDays(-20))
                    fi.Delete();
            }
        }
    }
}
