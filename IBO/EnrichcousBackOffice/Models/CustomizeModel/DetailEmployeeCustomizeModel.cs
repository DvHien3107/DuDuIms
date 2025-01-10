using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public partial class DetailEmployeeCustomizeModel
    {
        public string MemberNumber { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public decimal? BaseSalary { get; set; }
        public string Commission { get; set; }
        public string Total { get; set; }
    }
}