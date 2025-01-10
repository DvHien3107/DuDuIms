using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using System.Xml.Serialization;
using Enrichcous.Payment.Nuvei.Config.Models;
using Inner.Libs.Helpful;

namespace Enrichcous.Payment.Nuvei.Config
{
    public class Constant
    {
        public static string configPath = "";
        private static AppData _config;
        public static AppData Config
        {
            get
            {
                if (/*_config == null &&*/ configPath != "")
                {
                    var path = "";
                    try
                    {
                        path = HostingEnvironment.MapPath(configPath);
                    }
                    catch (Exception e)
                    {
                        path = configPath;
                    }
                    path = path ?? configPath;
                    try
                    {
                        XmlSerializer ser = new XmlSerializer(typeof(AppData));
                        using (StreamReader sr = new StreamReader(path))
                        {
                            _config = (AppData) ser.Deserialize(sr);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        throw new AppHandleException("Config file not found or invalid!");
                    }
                }
                return _config;
            }
        }

        public static List<Terminal> Terminals => Config?.Terminals;
        public static bool? AccountTest => Config?.AccountTest;
    }
}