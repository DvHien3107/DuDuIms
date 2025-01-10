using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Extensions.Helper
{
    public static class WriteLogHelper
    {
        public static void Error(string Message, string Type = "General")
        {
            try
            {
                string path = "C:\\Logs\\Error";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string filepath = "C:\\Logs\\Error\\" + Type + "_" + DateTime.UtcNow.Date.ToShortDateString().Replace('/', '_') + ".txt";
                if (!File.Exists(filepath))
                {
                    // Create a file to write to.   
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        sw.WriteLine(DateTime.UtcNow.ToString() + ": " + Message);
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
            catch { }
            
        }

        public static void Info(string Message, string Type = "General")
        {
            try
            {
                string path = "C:\\Logs\\Info";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string filepath = "C:\\Logs\\Info\\" + Type + "_" + DateTime.UtcNow.Date.ToShortDateString().Replace('/', '_') + ".txt";
                if (!File.Exists(filepath))
                {
                    // Create a file to write to.   
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        sw.WriteLine(DateTime.UtcNow.ToString() + ": " + Message);
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
            catch { }

        }

        public static void Success(string Message, string Type = "General")
        {
            try
            {
                string path = "C:\\Logs\\Success";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string filepath = "C:\\Logs\\Success\\" + Type + "_" + DateTime.UtcNow.Date.ToShortDateString().Replace('/', '_') + ".txt";
                if (!File.Exists(filepath))
                {
                    // Create a file to write to.   
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        sw.WriteLine(DateTime.UtcNow.ToString() + ": " + Message);
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
            catch { }

        }
    }
}
