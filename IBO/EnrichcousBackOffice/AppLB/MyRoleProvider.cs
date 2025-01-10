using EnrichcousBackOffice.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Security;

namespace EnrichcousBackOffice.AppLB
{
    public class MyRoleProvider : RoleProvider
    {
        public override string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetRolesForUser(string username)
        {
            WebDataModel db = new WebDataModel();
            var user = db.P_Member.Where(u => u.PersonalEmail.Equals(username)).FirstOrDefault();
            if (user == null)
            {
                return new string[] { };
            }
            else
            {
                var roleArr = new string[] { };
                var roleList = user.RoleCode?.Split(new char[] { ',' });
                if (roleList.Count() > 0)
                {
                    roleArr = new string[roleList.Count()];
                    int i = 0;
                    foreach (var item in roleList)
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                        {
                            roleArr[i] = item.Trim();
                            i++;
                        }
                        
                    }
                }
                
                return roleArr;
                
            }
        }



        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}