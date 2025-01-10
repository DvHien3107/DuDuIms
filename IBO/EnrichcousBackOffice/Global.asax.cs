using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using System.Globalization;
using System.Threading;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Optimization;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.AppLB;
using Enrich.Infrastructure.Data;
using Enrich.Web.Framework;
using Enrich.IServices;
using Enrich.Core.Infrastructure;
using Serilog;
using Serilog.Sinks.Graylog;

namespace EnrichcousBackOffice
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            CultureInfo ci = CultureInfo.CreateSpecificCulture("en-US");
            ci.DateTimeFormat.ShortDatePattern = "MM-dd-yyyy";
            CultureInfo.CurrentCulture = ci;
            CultureInfo.CurrentUICulture = ci;
            CultureInfo.DefaultThreadCurrentCulture = ci;

            //Init log

            var applicationName = ConfigurationManager.AppSettings["ApplicationName"];
            //Log.Logger  = new LoggerConfiguration()
            //  .WriteTo.Graylog(
            //    new GraylogSinkOptions
            //    {
            //        HostnameOrAddress = ConfigurationManager.AppSettings["HostName"],
            //        Port = int.Parse(ConfigurationManager.AppSettings["Port"]),
            //        TransportType = Serilog.Sinks.Graylog.Core.Transport.TransportType.Udp,
            //    }
            //  ).Enrich.WithProperty("ApplicationName", applicationName).MinimumLevel.Information().Enrich.FromLogContext().CreateLogger();

            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            DependencyResolver.SetResolver(new EnrichDependencyResolver());
            EnrichDataMapping.Start();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //SqlDependency.Start(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            // DataTables.AspNet registration with default options.
            DataTables.AspNet.Mvc5.Configuration.RegisterDataTables();
            HangfireBootstrapper.Instance.Start();
            HttpConfiguration config = GlobalConfiguration.Configuration;

            config.Formatters.JsonFormatter
                        .SerializerSettings
                        .ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        }
        protected void Application_Error()
        {
        //    var logger = EngineContext.Current.Resolve<ILogService>();
        
        //    Exception ex = Server.GetLastError();
        //    logger.Error(ex,ex.Message);

            WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
            Exception ex = Server.GetLastError();
            _writeLogErrorService.InsertLogError(ex);
        }

        protected void Application_End(object sender, EventArgs e)
       {
            //SqlDependency.Stop(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            HangfireBootstrapper.Instance.Stop();
        }



    }


}