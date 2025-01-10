using System;
using System.IO;

namespace EnrichcousBackOffice.Extensions
{
    public static class LogModel
    {
        public static void WriteLog(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\Review_" + DateTime.UtcNow.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine("--------------------" + DateTime.UtcNow.ToString() + ": " + Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(DateTime.UtcNow.ToString() + ": " + Message);
                }
            }
        }

        public static void WriteLogCheck(string StoreCode, string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\LogCheck";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + $"\\LogCheck\\WriteLogCheck_{StoreCode}_" + DateTime.UtcNow.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(DateTime.Now.ToString() + ": " + Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(DateTime.Now.ToString() + ": " + Message);
                }
            }
        }

    }
}