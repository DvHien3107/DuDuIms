using Enrich.Core.Ultils;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.AppLB.GoogleAuthorize;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Services;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Auth.OAuth2.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    public class AuthCallbackController : Google.Apis.Auth.OAuth2.Mvc.Controllers.AuthCallbackController
    {
        private string Email { get; set; }
        protected override Google.Apis.Auth.OAuth2.Mvc.FlowMetadata FlowData
        {
            get { return new AppFlowMetadata(""); }
        }

        public override async Task<ActionResult> IndexAsync(AuthorizationCodeResponseUrl authorizationCode, CancellationToken taskCancellationToken)
        {
            if (string.IsNullOrEmpty(authorizationCode.Code))
            {
                var errorResponse = new TokenErrorResponse(authorizationCode);

                return OnTokenError(errorResponse);
            }

            var returnUrl = Request.Url.ToString();
            returnUrl = returnUrl.Substring(0, returnUrl.IndexOf("?"));
            Session["EmailAuthorize"] = UserId;
            var result =  await Flow.ExchangeCodeForTokenAsync(UserId, authorizationCode.Code, returnUrl,
                taskCancellationToken);

           // var AuthResult = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata(HttpContext.User.Identity.Name)).AuthorizeAsync(CancellationToken.None);
            //var success = GoogleCalendarSyncer.SyncToGoogleCalendar(this);
           
            //CalendarHub.RefreshData();
            using (var db = new WebDataModel())
            {
                //if (AuthResult.Credential == null)
                //{
                //    var member = db.P_Member.FirstOrDefault(x => x.PersonalEmail == UserId);
                //    member.IsAuthorizedGoogle = false;
                //    member.GoogleAuth = null;
                //    db.SaveChanges();
                //    return Content("Token was revoked. Try again.");
                //}
                //else
                //{
                    var member = db.P_Member.FirstOrDefault(x => x.PersonalEmail == UserId);
                    member.IsAuthorizedGoogle = true;
                    //var calendarDemoschedulerService = new CalendarDemoSchedulerService(AuthResult.Credential);
                    //calendarDemoschedulerService.WatchEvent();
                    db.SaveChanges();
                //}
            }
                return Redirect("/memberMan/VerifyGoogleAuth?key=" + SecurityLibrary.Encrypt(UserId));
        }
    }
}