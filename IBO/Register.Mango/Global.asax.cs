using Enrich.Infrastructure.Data;
using Enrich.Web.Framework;
using EnrichcousBackOffice.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Register.Mango
{
    public class MvcApplication : System.Web.HttpApplication
    {

        protected void Application_Start()
        {
            CultureInfo ci = CultureInfo.CreateSpecificCulture("en-US");
            ci.DateTimeFormat.ShortDatePattern = "MM-dd-yyyy";
            CultureInfo.CurrentCulture = ci;
            CultureInfo.CurrentUICulture = ci;
            CultureInfo.DefaultThreadCurrentCulture = ci;
            DependencyResolver.SetResolver(new EnrichDependencyResolver());
            EnrichDataMapping.Start();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_Error()
        {
            WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
            Exception ex = Server.GetLastError();
            _writeLogErrorService.InsertLogError(ex);
        }
    }
}
