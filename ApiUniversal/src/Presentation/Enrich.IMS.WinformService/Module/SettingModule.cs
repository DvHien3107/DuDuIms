using RunAtTime.Const;
using RunAtTime.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunAtTime.Module
{
    public class SettingModule
    {
        public static string TimeRun = "";
        public static string DomainNextGenApi = "";
        public static List<DomainRecurring> listRunApi = new List<DomainRecurring>();
        public static void InitMISetting()
        {
            try
            {
                string[] lines = File.ReadAllLines(Constant.staticSettingPath+ "\\Setting.txt");
                string line = "";
                foreach (var it in lines)
                {
                    line = it.Trim();
                    if (line != null && line.Trim().Length > 0)
                    {
                        if (line.StartsWith("TimeRun:"))
                        {
                            TimeRun = line.Replace("TimeRun:", "");
                        }
                        if (line.StartsWith("DomainNextGenApi:"))
                        {
                            DomainNextGenApi = line.Replace("DomainNextGenApi:", "");
                        }
                    }
                }
                InitListRunApi();
            }
            catch (Exception e)
            {
                LogModule.AddLogError("--InitMISetting--" + DateTime.UtcNow + Environment.NewLine + e.ToString());
            }
        }

        public static void InitListRunApi()
        {
            try
            {
                string[] lines = File.ReadAllLines(Constant.staticSettingPath + "\\Setting.txt");
                string line = "";
                bool startListApi = false;
                foreach (var it in lines)
                {
                    line = it.Trim();
                    if (line != null && line.Trim().Length > 0)
                    {
                        if (line.StartsWith("--EndLstApi--"))
                        {
                            startListApi = false;
                        }
                        if (startListApi)
                        {
                            string[] apiUrl = line.Split('|');
                            DomainRecurring RunApi = new DomainRecurring();
                            RunApi.Domain = apiUrl[1];
                            RunApi.RecurringMinute = int.Parse(apiUrl[0]);
                            listRunApi.Add(RunApi);
                        }
                        if (line.StartsWith("--StartLstApi--"))
                        {
                            startListApi = true;
                        }

                    }
                }
            }
            catch (Exception e)
            {
                LogModule.AddLogError("--InitListRunApi--" + DateTime.UtcNow + Environment.NewLine + e.ToString());
            }
        }
    }
}
