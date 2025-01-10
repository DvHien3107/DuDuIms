using Enrich.Core.Infrastructure;
using Enrich.IServices.Utils.OAuth;
using EnrichcousBackOffice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebGrease.Css.Extensions;

namespace EnrichcousBackOffice.AppLB
{
    /// <summary>
    /// Xac thuc thong tin va phan quyen nguoi dung
    /// </summary>
    public class Authority
    {

        private static string session_access = "access_role";
       /// <summary>
       /// get quyen truy cap cac chuc nang.
       /// </summary>
       /// <param name="functionCode">chuc nang</param>
       /// <param name="memberRole">vai tro thanh vien</param>
       /// <param name="reload">kiem tra lai</param>
       /// <returns></returns>
        public static Dictionary<string, bool> GetAccessAuthority(bool reload = false)
        {
            var access = new Dictionary<string, bool>();
            //return access;
            try
            {
                var curMem = GetCurrentMember();
                if (curMem == null)
                {
                    return access;
                }
                string memberRole = curMem.RoleCode;

                if (HttpContext.Current.Session[session_access] == null || reload == true)
                {
                    WebDataModel db = new WebDataModel();
                    foreach (var itemRole in memberRole.Split(new char[] { ',' }).ToList())
                    {
                        if (!string.IsNullOrWhiteSpace(itemRole))
                        {
                            foreach (var item in db.A_GrandAccess.Where(p => p.RoleCode.Equals(itemRole.Trim())).OrderBy(p => p.FunctionCode).ToList())
                            {
                                if (access.ContainsKey(item.FunctionCode) == true)
                                {
                                    if (item.Access == true && access[item.FunctionCode] != true)
                                    {
                                        access[item.FunctionCode] = true;
                                    }
                                }
                                else
                                {
                                    access.Add(item.FunctionCode, item.Access ?? false);
                                }
                            }
                        }
                    }
                    return access;
                }
                else
                {
                    return HttpContext.Current.Session[session_access] as Dictionary<string, bool>;
                }
            }
            catch (Exception)
            {
                return access;
            }
           
        }

        public static Dictionary<string, bool> GetAccessAuthorityByMember(P_Member curMem)
        {
            var access = new Dictionary<string, bool>();

            try
            {
             
                if (curMem == null)
                {
                    return access;
                }
                string memberRole = curMem.RoleCode;

               
                    WebDataModel db = new WebDataModel();

                    foreach (var itemRole in memberRole.Split(new char[] { ',' }).ToList())
                    {
                        if (!string.IsNullOrWhiteSpace(itemRole))
                        {
                            foreach (var item in db.A_GrandAccess.Where(p => p.RoleCode.Equals(itemRole.Trim())).OrderBy(p => p.FunctionCode).ToList())
                            {
                                if (access.ContainsKey(item.FunctionCode) == true)
                                {
                                    if (item.Access == true && access[item.FunctionCode] != true)
                                    {
                                        access[item.FunctionCode] = true;
                                    }

                                }
                                else
                                {
                                    access.Add(item.FunctionCode, item.Access ?? false);
                                }

                            }
                        }
                    }


                    return access;
                
            }
            catch (Exception)
            {
                return access;
            }

        }
        public static Dictionary<string, Action<P_Member>> observable { get; } = new Dictionary<string, Action<P_Member>>();
        /// <summary>
        /// get thong tin member hien tai
        /// </summary>
        /// <returns></returns>
        public static P_Member GetCurrentMember(bool reload = false)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                P_Member member = null;
                if (HttpContext.Current?.Session["member"] != null)
                {
                    member = HttpContext.Current.Session["member"] as P_Member;

                }

                if (member == null)
                {
                    if (HttpContext.Current?.User.Identity.IsAuthenticated == true)
                    {
                        string email = HttpContext.Current.User.Identity.Name;
                        var mem = db.P_Member.Where(m => m.PersonalEmail.Equals(email) && m.Active == true).FirstOrDefault();
                        if (mem != null)
                        {
                            HttpContext.Current.Session["member"] = mem;
                            string traceTrackId = Guid.NewGuid().ToString();
                            HttpContext.Current.Session["traceTrackId"] = traceTrackId;
                            member = mem;
                            var _enrichOAuth = EngineContext.Current.Resolve<IEnrichOAuth>();
                            var accessToken = _enrichOAuth.GetAccessToken(mem.PersonalEmail, mem.Password);
                            HttpContext.Current.Session["ACCESS_TOKEN"] = accessToken;
                        }
                    }
                }
                //else if (!HttpContext.Current.User.Identity.IsAuthenticated)
                //{
                //    System.Web.Security.FormsAuthentication.SetAuthCookie(member.PersonalEmail, true);
                //}

                if (member != null && reload == true)
                {
                    member = db.P_Member.Find(member.Id);
                    HttpContext.Current.Session["member"] = member;
                    string traceTrackId = Guid.NewGuid().ToString();
                    HttpContext.Current.Session["traceTrackId"] = traceTrackId;
                }
                observable.ForEach(pair => pair.Value(member));
                if (member?.PersonalEmail != HttpContext.Current?.User.Identity.Name && !string.IsNullOrWhiteSpace(HttpContext.Current?.User.Identity.Name))
                {
                    //HttpContext.Current.Response.Redirect("/Account/Logout/email_change", false);
                }
                if (member != null && member?.Active != true)
                {
                    //HttpContext.Current.Response.Redirect("/Account/Logout/deactive", false);
                }
                return member;
            }
            catch (Exception)
            {
                return new P_Member {FullName = "Other Member"};
            }

        }

      
    }
}