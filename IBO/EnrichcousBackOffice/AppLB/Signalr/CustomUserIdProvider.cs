using EnrichcousBackOffice.Models;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.AppLB.Signalr
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(IRequest request)
        {
            // your logic to fetch a user identifier goes here.

            // for example:

            var email = request.User.Identity.Name;
            var db = new WebDataModel();
            var member = db.P_Member.FirstOrDefault(x => x.PersonalEmail == email);
            return member.MemberNumber;
        }
    }
}