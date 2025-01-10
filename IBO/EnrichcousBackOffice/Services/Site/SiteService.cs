using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Services.Site
{
    public class SiteService
    {
        public List<A_Role> GetRolesBySiteId(int siteId,bool getAll=false)
        {
            using (WebDataModel db = new WebDataModel())
            {
                var roles = from siteRole in db.SiteRoles join role in db.A_Role on siteRole.RoleId equals role.Id orderby role.RoleLevel descending select new { siteRole, role };
                if (getAll==false)
                {
                    roles = roles.Where(x => x.siteRole.SiteId == siteId);
                }
                return roles.Select(x => x.role).ToList();
            }
        }
        public List<A_FunctionInPage> GetFunctionBySite(int siteId)
        {
            using (WebDataModel db = new WebDataModel())
            {
                var allFunction = from func in db.A_FunctionInPage select func;
                if (siteId != 1)
                {
                     allFunction = from siteFunc in db.SiteFunctions join func in db.A_FunctionInPage on siteFunc.FunctionId equals func.Id where siteFunc.SiteId == siteId orderby func.Order select func;
                }
                return allFunction.ToList();
            }
        }
        public List<A_GrandAccess> GetGrandAccessRoleCode(string RoleCode)
        {
            using (WebDataModel db = new WebDataModel())
            {
                var access = db.A_GrandAccess.Where(x => x.RoleCode == RoleCode).ToList();
                return access;
            }
        }
    }
}