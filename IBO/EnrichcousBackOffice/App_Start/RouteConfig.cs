using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EnrichcousBackOffice
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //name: "flash-ticket",
            //url: "Ticket/{action}/{id}",
            //defaults: new { Areas="Page",Controller = "Ticket", action = "Index", id = UrlParameter.Optional },
            //namespaces: new string[] { "EnrichcousBackOffice.Areas.Page.Controllers" }
            // );
        //    routes.MapRoute(
        //name: "flash-ticket",
        //url: "Ticket/{action}/{id}",
        //defaults: new { Controller = "Ticket", action = "Index", id = UrlParameter.Optional },
        //namespaces: new string[] { "EnrichcousBackOffice.Areas.Page.Controllers" }
        // ).DataTokens.Add("area", "Page"); ;

            routes.MapRoute(
             name: "ticket",
             url: "ticket/{action}/{id}",
             defaults: new {  Controller = "Ticket_New", action = "Index", id = UrlParameter.Optional }
              );


            routes.MapRoute(
                name: "flash-ticket",
                url: "quick-ticket/{action}/{id}",
                defaults: new { Controller = "Ticket", action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "EnrichcousBackOffice.Areas.Page.Controllers" }
                 ).DataTokens.Add("area", "Page"); ;
            // #region development ticket
            // routes.MapRoute(
            //    name: "development",
            //    url: "development",
            //    defaults: new { Controller = "development_new", action = "index" },
            //    namespaces: new string[] { "EnrichcousBackOffice.Controllers" }
            // );
            // routes.MapRoute(
            //    name: "development-action",
            //    url: "development/{action}/{id}",
            //    defaults: new { Controller = "development_new", action = "action", id = "id" },
            //    namespaces: new string[] { "EnrichcousBackOffice.Controllers" }
            // );
            // routes.MapRoute(
            //    name: "development-update",
            //    url: "development/update/{id}",
            //    defaults: new { Controller = "development_new", action = "update", id = UrlParameter.Optional },
            //    namespaces: new string[] { "EnrichcousBackOffice.Controllers" }
            // );

            // routes.MapRoute(
            //   name: "development-detail",
            //   url: "development/detail/{id}",
            //   defaults: new { Controller = "development_new", action = "detail", id = UrlParameter.Optional },
            //   namespaces: new string[] { "EnrichcousBackOffice.Controllers" }
            //);

            // #endregion


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "EnrichcousBackOffice.Controllers" }
            );

        }
    }
}
