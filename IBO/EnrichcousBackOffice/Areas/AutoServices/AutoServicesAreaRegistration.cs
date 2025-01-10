using System.Web.Mvc;

namespace EnrichcousBackOffice.Areas.AutoServices
{
    public class AutoServicesAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "AutoServices";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "AutoServices_default",
                "AutoServices/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}