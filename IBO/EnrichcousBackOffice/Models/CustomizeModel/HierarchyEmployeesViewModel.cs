using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class HierarchyEmployeesViewModel
    {
       
        public HierarchyEmployeesViewModel()
        {
            this.ChildEmployees = new HashSet<HierarchyEmployeesViewModel>();
        }

        public string MemberNumber { get; set; }
        public string FullName { get; set; }
        public string ReferedByMemberNumber { get; set; }
        //active: true/ false| null: pending approve
        public bool? Active { get; set; }

        public virtual ICollection<HierarchyEmployeesViewModel> ChildEmployees { get; set; }
       

    }
}