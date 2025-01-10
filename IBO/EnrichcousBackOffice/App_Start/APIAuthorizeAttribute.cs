using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Enrich.Core.Ultils;
using EnrichcousBackOffice.Models;

namespace EnrichcousBackOffice.App_Start
{
    public class APIAuthorizeAttribute : AuthorizeAttribute
    {
        private WebDataModel db = new WebDataModel();
        public override void OnAuthorization(HttpActionContext filterContext)
        {
            if (Authorize(filterContext))
            {
                return;
            }
            if (SkipAuthorization(filterContext))
            {
                return;
            }
            HandleUnauthorizedRequest(filterContext);
        }


        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            base.HandleUnauthorizedRequest(actionContext);
        }



        private bool Authorize(HttpActionContext filterContext)
        {
            try
            {
                var encodedString = filterContext.Request.Headers.GetValues("Authorization").First();
                bool validFlag = false;

                if (!string.IsNullOrEmpty(encodedString))
                {
                    //validFlag = false;

                    //co the ap dung co che token khac, khong can get token = cach dang nhap.
                    //truong hop nay token duoc gui len la encrypt(enrich|datetime.UtcNow.tick)
                    //neu decrypt kiem tra neu tick cach thoi diem hien tai <= 3min la ok
                    var token_decrypt = SecurityLibrary.Decrypt(encodedString);
                    var token_arr = token_decrypt.Split(new char[] { '|' });
                    string username = token_arr[0];
                    string password = token_arr[1];
                    //TimeSpan from_timespan = new TimeSpan(long.Parse(token_arr[1]));
                    //TimeSpan now_timespan = new TimeSpan(DateTime.UtcNow.Ticks);

                    //double fromMinute = Math.Floor(from_timespan.TotalMinutes);
                    //double nowMinute = Math.Floor(now_timespan.TotalMinutes);

                    if (username == "ims@enrichcous.com" && password == DateTime.Today.ToString("yyyyMMdd"))
                    {
                        validFlag = true;
                    }
                    else
                    {
                        validFlag = false;
                    }
                }

                return validFlag;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool SkipAuthorization(HttpActionContext actionContext)
        {
            Contract.Assert(actionContext != null);
            return actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any()
                   || actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any();
        }
    }
}