using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnrichcousBackOffice.Models;

namespace EnrichcousBackOffice.ViewControler
{
    public class EmployeesViewService
    {
        // GET: EmployeesView

        /// <summary>
        /// Get member in department
        /// </summary>
        /// <param name="id">Department id</param>
        /// <returns>list member number</returns>
        public static List<string> GetMemberNumberInDept(long id, bool? active = null)
        {
            WebDataModel db = new WebDataModel();
            List<string> memDeptID = new List<string>();
            string _id = id.ToString();
            foreach (var item in db.P_Member.Where(e => e.DepartmentId.Contains(_id) && (active == null || e.Active == active)).ToList())
            {
                memDeptID.Add(item.MemberNumber);
            }

            return memDeptID;
        }

      


        /// <summary>
        /// Get list member number in team
        /// </summary>
        /// <param name="id">Department id</param>
        /// <returns>list member number</returns>
        public static List<string> GetMemberNumberInBranch(string memberNumber)
        {
            WebDataModel db = new WebDataModel();
            List<string> memNumberInBranch = new List<string>();
            
            IEnumerable<P_Member> mem = db.P_Member.Where(m => m.ReferedByNumber == memberNumber && m.Active == true).ToList();

            if (mem.Count() == 0)
            {
                memNumberInBranch.Add(memberNumber);
                return memNumberInBranch;
            }
            foreach (var item in mem)
            {
                memNumberInBranch.AddRange(GetMemberNumberInBranch(item.MemberNumber));
            }

            memNumberInBranch.Add(memberNumber);
            return memNumberInBranch;
        }
    }
}