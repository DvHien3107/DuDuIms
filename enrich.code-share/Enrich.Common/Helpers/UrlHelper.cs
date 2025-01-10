using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Common.Helpers
{
    public static class UrlHelper
    {       
        public static string GetLastUrlSegment(Uri uri)
        {
            return uri.Segments.LastOrDefault();
        }

        /// <summary>
        /// http://abc.com -> abc.com, https://xyz.com -> xyz.com
        /// </summary>
        public static string OptimizeDomain(this string originalDomain)
        {
            //new Uri(url2, UriKind.RelativeOrAbsolute).Host
            var domain = originalDomain.Replace("http:", "").Replace("https:", "").Trim('/', '\\');
            var indexPort = domain.IndexOf(":");

            return indexPort > 0 ? domain.Substring(0, indexPort) : domain;
        }

        public static string GetFullUrl(string url, bool hasLastSeparate)
        {
            string strTemp = url;
            int intIndex = strTemp.LastIndexOf(@"/");

            //Vy Add for https:// case
            if (strTemp.IndexOf(@"http://", StringComparison.OrdinalIgnoreCase) != 0 && strTemp.IndexOf(@"https://", StringComparison.OrdinalIgnoreCase) != 0)
                strTemp = "http://" + strTemp;

            if (hasLastSeparate && strTemp.Length - 1 != intIndex)
                strTemp += "/";

            return strTemp;
        }

        public static string GetFTPUrl(string ftpServer, bool hasLastSeparate = true)
        {
            if (string.IsNullOrEmpty(ftpServer))
                return string.Empty;

            ftpServer = ftpServer.Trim();

            if (ftpServer.IndexOf("ftp://", StringComparison.OrdinalIgnoreCase) == -1)
            {
                ftpServer = $"ftp://{ftpServer}";
            }

            if (hasLastSeparate && !ftpServer.EndsWith("/"))
            {
                ftpServer = $"{ftpServer}/";
            }

            return ftpServer;
        }

        public static string AddQueryString(this string resource, string name, string value) => AddQueryString(resource, (name, value));

        public static string AddQueryString(this string resource, params (string Name, string Value)[] queryStrings)
        {
            if (queryStrings.Length == 0)
            {
                return resource;
            }

            var separate = resource.IndexOf("?") > 0 ? "&" : "?";

            return $"{resource.TrimEnd('/', '&', '?')}{separate}{queryStrings.Select(a => a.Value == null ? a.Name : $"{a.Name}={a.Value}").ToStringList("&")}";
        }
    }
}
