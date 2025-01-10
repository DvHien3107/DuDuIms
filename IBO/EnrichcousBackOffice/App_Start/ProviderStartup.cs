using EnrichcousBackOffice.Areas.PaymentGate.Services;
using EnrichcousBackOffice.Services.Employees;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Services.NextGen.Mail;
using EnrichcousBackOffice.Services.Repository;
using EnrichcousBackOffice.Services.Site;
using Google.Apis.Services;
using iTextSharp.tool.xml.pipeline.ctx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.ApplicationServices;
using System.Web.Mvc;
using Unity;
using Unity.Lifetime;

namespace EnrichcousBackOffice.App_Start
{
    public static class IocExtensions
    {
        public static void BindInRequestScope<T1, T2>(this IUnityContainer container) where T2 : T1
        {
            container.RegisterType<T1, T2>(new HierarchicalLifetimeManager());
        }
        public static void BindInRequestScope<T1>(this IUnityContainer container)
        {
            container.RegisterType<T1>(new HierarchicalLifetimeManager());
        }

    }
    public static class UnityHelper
    {
        public static IUnityContainer Container;
        //public static void InitialiseUnityContainer()
        //{
        //    Container = new UnityContainer();
        //    DependencyResolver.SetResolver(new UnityDependencyResolver(Container));

        //    // Bit annoying having just this here but we need this early in the startup for seed method
        //    Container.BindInRequestScope<IConfigService, ConfigService>();
        //    Container.BindInRequestScope<ICacheService, CacheService>();

        //}
    }
    public static class ProviderStartup
    {

        public static void BuildUnityContainer()
        {
            UnityContainer Container = new UnityContainer();
            //Container.BindInRequestScope<IEmailService, EmailService>();
            //Container.BindInRequestScope<ICustomerRepository, CustomerRepository>();
            //Container.BindInRequestScope<IOrderRepository, OrderRepository>();

        }
    }
}
