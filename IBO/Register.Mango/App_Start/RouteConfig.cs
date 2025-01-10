using System.Web.Mvc;
using System.Web.Routing;

namespace Register.Mango
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
             name: "Register",
             url: "register",
             defaults: new { controller = "Register", action = "Index" });

            routes.MapRoute(
               name: "Thanks",
               url: "thanks",
               defaults: new { controller = "Register", action = "Thanks" });

            routes.MapRoute(
              name: "Verify",
              url: "verify",
              defaults: new { controller = "Register", action = "Verify" });
            routes.MapRoute(
             name: "VerifyForEmail",
             url: "verifyforemail",
             defaults: new { controller = "Register", action = "VerifyForEmail" });

            routes.MapRoute(
           name: "UpdateInfoStore",
           url: "updateInfoStore",
           defaults: new { controller = "Register", action = "UpdateInfoStore" });

            routes.MapRoute(
            name: "Subscribe",
            url: "subscribe/license_code={license_code}",
            defaults: new { controller = "Subscribe", action = "Index", license_code = UrlParameter.Optional });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new string[] { "Register.Mango.Controllers" }
            );
        }
    }
}
