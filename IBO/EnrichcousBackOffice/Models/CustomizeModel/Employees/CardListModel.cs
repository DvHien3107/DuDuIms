using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Employees
{
    public partial class CardListModel
    {
        public int Id { get; set; }
        public string CardNumber { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public string Note { get; set; }
        public string BankName { get; set; }
        public string BranchNameBank { get; set; }
        public bool? IsDefault { get; set; }
    }
}
