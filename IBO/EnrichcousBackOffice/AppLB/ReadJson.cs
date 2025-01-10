using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace EnrichcousBackOffice.AppLB
{
    public class ReadJson
    {
        public T ReadDataFromFile<T>(string path) where T : class
        {
            using (StreamReader r = new StreamReader(HostingEnvironment.MapPath("/")+path))
            {
                string json = r.ReadToEnd();
               T result = JsonConvert.DeserializeObject<T>(json);
                return result;
            }
            
        }
    }
}