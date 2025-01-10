using System.Web.Hosting;
using System.Web.Mvc;
using Enrichcous.Payment.Mxmerchant.Config;

namespace EnrichcousBackOffice.Areas.PaymentGate
{
    public class PaymentGateAreaRegistration : AreaRegistration 
    {
        public override string AreaName => "PaymentGate";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            //Constant.configPath = HostingEnvironment.MapPath("/App_Data/Nuvei/Config.xml");
            context.MapRoute(
                "PaymentGate_default",
                "PaymentGate/{controller}/{action}/{id}",
                new { Controller = "Purchases", action = "Index" ,id = UrlParameter.Optional }
            );
            context.MapRoute(
                "PaymentGate_IndexMap",
                "PaymentGate/{controller}",
                new { Controller = "Purchases", action = "Index" }
            );
        }
    }
}