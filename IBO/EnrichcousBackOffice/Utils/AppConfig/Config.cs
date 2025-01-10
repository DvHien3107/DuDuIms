using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Xml.Serialization;
using Inner.Libs.Helpful;

namespace EnrichcousBackOffice.Utils.AppConfig
{
    public static class AppConfig
    {
        public static string configPath = HostingEnvironment.MapPath("/App_Data/Config.xml");
        private static AppData _cfg;
        public static AppData Cfg
        {
            get
            {
                if (_cfg == null && configPath != "")
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
                            _cfg = (AppData) ser.Deserialize(sr);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        throw new AppHandleException("Config file not found or invalid!");
                    }
                }
                return _cfg;
            }
        }

        public static string Host
        {
            get
            {
                try
                {
                    var scheme = HttpContext.Current?.Request.Url.Scheme;
                    var authority = HttpContext.Current?.Request.Url.Authority;
                    var host = $"{scheme}://{authority}";
                    return host;
                }
                catch (Exception)
                {
                    return ConfigurationManager.AppSettings["IMSUrl"];
                }
            }
        }
    }
}