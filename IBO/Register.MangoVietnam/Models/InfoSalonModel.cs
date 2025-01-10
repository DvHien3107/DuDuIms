using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Register.MangoVietnam.Models
{
    public class InfoSalonModel
    {
        public string SalonName { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public MoreInfo MoreInfo { get; set; }
        public string Address { get; set; }

    }
    public class MoreInfo
    {
        public string NumberBranches { get; set; }
        public string NumberEmployees { get; set; }
        //bạn đã hoặc đang sử dụng phần mềm quản lý
        public string AreUsingSoftware { get; set; }
        //phần cứng
        public string Hardware { get; set; }
        public string ServicePackage { get; set; }
    }
}