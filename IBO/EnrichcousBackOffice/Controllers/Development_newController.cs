using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.ViewControler;
using EnrichcousBackOffice.ViewModel;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    [MyAuthorize]
    public class Development_newController : UploadController
    {
        private Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();
        private P_Member cMem = AppLB.Authority.GetCurrentMember();
        private WebDataModel db = new WebDataModel();
        public ActionResult Detail(long? id)
        {
            return Redirect("/Ticket_New/Detail/" + id);
        }
    }
}