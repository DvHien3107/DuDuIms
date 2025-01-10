using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Payroll
{
    public class PayslipUpLoadModel
    {
        public int? Id { get; set; }
        public DateTime? CreateAt { get; set; }
        public string CreateBy { get; set; }
        public string FilePath { get; set; }
        public DateTime? PayrollDate { get; set; }
        public bool? IsExist { get; set; }
        public string FileName { get; set; }
        public string EmployeeId  { get; set; }
        public string EmployeeName  { get; set; }
    }
}