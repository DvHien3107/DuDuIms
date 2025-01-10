using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Hangfire;
using Owin;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.AppLB.Signalr;
using Microsoft.AspNet.SignalR;
using EnrichcousBackOffice.NextGen;
using EnrichcousBackOffice.App_Start;

[assembly: OwinStartup(typeof(EnrichcousBackOffice.Startup))]

namespace EnrichcousBackOffice
{

    public class Startup
    {
       
        public void Configuration(IAppBuilder app)
        {
            // Provider 
            ProviderStartup.BuildUnityContainer();
        }
    }
}
