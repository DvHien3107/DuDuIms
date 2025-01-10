using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
   
        public class Provinces
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
        }
        public class Districts
        {
            public int Province_Id { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public class Wards
        {
            public int District_Id { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public string Prefix { get; set; }
        }
    
}