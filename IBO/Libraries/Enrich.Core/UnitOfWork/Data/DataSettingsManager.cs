using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;

namespace Enrich.Core.UnitOfWork.Data
{
    public partial class DataSettingsManager
    {
        protected const char separator = ':';

        /// <summary>
        /// Parse settings
        /// </summary>
        /// <param name="text">Text of settings file</param>
        /// <returns>Parsed data settings</returns>
        protected virtual DataSettings ParseSettings(string text)
        {
            var shellSettings = new DataSettings();
            if (String.IsNullOrEmpty(text))
                return shellSettings;


            shellSettings.DataProvider = "sqlserver";
            shellSettings.DataConnectionString = GetConectionString();
            return shellSettings;
        }

        /// <summary>
        /// get connection
        /// </summary>
        /// <returns></returns>
        public static string GetConectionString()
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        /// <summary>
        /// Convert data settings to string representation
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <returns>Text</returns>
        protected virtual string ComposeSettings(DataSettings settings)
        {
            if (settings == null)
                return "";

            return string.Format("DataProvider: {0}{2}DataConnectionString: {1}{2}",
                                 settings.DataProvider,
                                 settings.DataConnectionString,
                                 Environment.NewLine
                );
        }

        /// <summary>
        /// Maps a virtual path to a physical disk path.
        /// </summary>
        /// <param name="path">The path to map. E.g. "~/bin"</param>
        /// <returns>The physical path. E.g. "c:\inetpub\wwwroot\bin"</returns>
        public static string MapPath(string path)
        {
            if (HostingEnvironment.IsHosted)
            {
                //hosted
                return HostingEnvironment.MapPath(path);
            }

            //not hosted. For example, run in unit tests
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            path = path.Replace("~/", "").TrimStart('/').Replace('/', '\\');
            return Path.Combine(baseDirectory, path);
        }

        /// <summary>
        /// Load settings
        /// </summary>
        /// <param name="filePath">File path; pass null to use default settings file path</param>
        /// <returns></returns>
        public virtual DataSettings LoadSettings()
        {
            var settings = new DataSettings();
            settings.DataProvider = "sqlserver";
            settings.DataConnectionString = GetConectionString();
            return settings;
        }

    }
}
