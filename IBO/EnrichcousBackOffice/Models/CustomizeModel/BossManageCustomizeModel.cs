using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class BossManageReq
    {
        public string idBossManage { get; set; }
        public string owner { get; set; }
        public string contactPerson { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string comments { get; set; }
    }

    public class BossManageRes
    {
        public string result { get; set; }
        public string message { get; set; }
        public string idBossManage { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string urlConnect { get; set; }
    }
}