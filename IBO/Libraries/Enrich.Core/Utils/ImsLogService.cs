using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Core.Ultils
{
    public static class ImsLogService
    {
        public static void WriteLog(string Message, string Note = "Log", string Path = "General")
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\" + Path;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\" + Path + "\\Review_" + DateTime.UtcNow.Date.ToShortDateString().Replace('/', '_') + ".txt";
                if (!File.Exists(filepath))
                {
                    // Create a file to write to.   
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        sw.WriteLine("\r\n");
                        sw.WriteLine(DateTime.UtcNow.ToString() + $" - {Note} -: " + Message);
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText(filepath))
                    {
                        sw.WriteLine("\r\n");
                        sw.WriteLine(DateTime.UtcNow.ToString() + $" - {Note} -: " + Message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
    }
}
