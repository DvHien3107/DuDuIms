using Autofac;
using Autofac.Integration.Mvc;
using Enrich.Core;
using Enrich.Core.Infrastructure;
using Enrich.Core.Infrastructure.DependencyManagement;
using Enrich.Core.UnitOfWork;
using Enrich.Core.UnitOfWork.Data;
using Enrich.DataTransfer;
using Enrich.Entities;
using Enrich.Infrastructure.Data;
using Enrich.IServices;
using Enrich.IServices.Utils;
using Enrich.IServices.Utils.EnrichUniversal;
using Enrich.IServices.Utils.GoHighLevelConnector;
using Enrich.IServices.Utils.JiraConnector;
using Enrich.IServices.Utils.Mailing;
using Enrich.IServices.Utils.OAuth;
using Enrich.IServices.Utils.SMS;
using Enrich.IServices.Utils.Universal;
using Enrich.Services;
using Enrich.Services.Utils;
using Enrich.Services.Utils.EnrichUniversal;
using Enrich.Services.Utils.GoHighLevelConnector;
using Enrich.Services.Utils.JiraConnector;
using Enrich.Services.Utils.Mailing;
using Enrich.Services.Utils.OAuth;
using Enrich.Services.Utils.SMS;
using Enrich.Services.Utils.Universal;
using EnrichcousBackOffice.NextGen;
using System;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Security;

namespace Enrich.Web.Framework
{
    public class DependencyRegistra : IDependencyRegistrar
    {
        private bool _isDebug;
        private readonly string Environment = ConfigurationManager.AppSettings["Environment"];

        private readonly string Production = "Production";
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            DomainConfig.NextGenApi = ConfigurationManager.AppSettings["POSNextGenApi"];

#if DEBUG
            _isDebug = true;
#endif
            // Register the Web API controllers.
            //  builder.RegisterApiControllers(typeFinder.GetAssemblies().ToArray());
            //controllers
            builder.RegisterControllers(typeFinder.GetAssemblies().ToArray());
            builder.Register(c => new EnrichContext()).As<EnrichContext>().InstancePerLifetimeScope();
            builder.Register(c => c.Resolve<TimeSpan>()).As<TimeSpan>().InstancePerLifetimeScope();
            builder.Register(c => c.Resolve<HttpContextBase>().Request).As<HttpRequestBase>().InstancePerLifetimeScope();
            builder.Register(c => c.Resolve<HttpContextBase>().Response).As<HttpResponseBase>().InstancePerLifetimeScope();
            builder.Register(c => c.Resolve<HttpContextBase>().Server).As<HttpServerUtilityBase>().InstancePerLifetimeScope();
            builder.Register(c => c.Resolve<HttpContextBase>().Session).As<HttpSessionStateBase>().InstancePerLifetimeScope();

            //data layer
            var dataSettingsManager = new DataSettingsManager();
            var dataProviderSettings = dataSettingsManager.LoadSettings();
            builder.Register(c => dataSettingsManager.LoadSettings()).As<DataSettings>();

            if (dataProviderSettings != null && dataProviderSettings.IsValid())
            {
                var efDataProviderManager = new EfDataProviderManager(dataSettingsManager.LoadSettings());
                var dataProvider = efDataProviderManager.LoadDataProvider();
                dataProvider.InitConnectionFactory();

                builder.Register<IDbContext>(c => new EnrichObjectContext(dataProviderSettings.DataConnectionString)).InstancePerLifetimeScope();
            }
            else
            {
                builder.Register<IDbContext>(c => new EnrichObjectContext(dataSettingsManager.LoadSettings().DataConnectionString)).InstancePerLifetimeScope();
            }

            builder.RegisterGeneric(typeof(EfRepository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();

            //SqlProvider
            builder.RegisterType<SqlProviderService>().As<ISqlProviderService>().InstancePerLifetimeScope();
            builder.RegisterType<EmailTemplateService>().As<IEmailTemplateService>().InstancePerLifetimeScope();
            builder.RegisterType<MerchantService>().As<IMerchantService>().InstancePerLifetimeScope();
            builder.RegisterType<SMSService>().As<ISMSService>().InstancePerLifetimeScope();
            builder.RegisterType<EnrichSMSService>().As<IEnrichSMSService>().InstancePerLifetimeScope();
            builder.RegisterType<MailingService>().As<IMailingService>().InstancePerLifetimeScope();
            builder.RegisterType<EnrichEmailService>().As<IEnrichEmailService>().InstancePerLifetimeScope();
            builder.RegisterType<TwilioRestApiService>().As<ITwilioRestApiService>().InstancePerLifetimeScope();
            builder.RegisterType<EnrichUniversalService>().As<IEnrichUniversalService>().InstancePerLifetimeScope();
            builder.RegisterType<TicketUniversalService>().As<ITicketUniversalService>().InstancePerLifetimeScope();
            builder.RegisterType<GoHighLevelConnectorService>().As<IGoHighLevelConnectorService>().InstancePerLifetimeScope();
			builder.RegisterType<NewCustomerGoalService>().As<INewCustomerGoalService>().InstancePerLifetimeScope();
            builder.RegisterType<ConnectorJiraService>().As<IConnectorJiraService>().InstancePerLifetimeScope();
            builder.RegisterType<JiraConnectorAuthService>().As<IJiraConnectorAuthService>().InstancePerLifetimeScope();
            builder.RegisterType<EnrichOAuth>().As<IEnrichOAuth>().InstancePerLifetimeScope();
            builder.RegisterType<PosSyncTwilioApiService>().As<IPosSyncTwilioApiService>().InstancePerLifetimeScope();
			builder.RegisterType<ClickUpConnectorService>().As<IClickUpConnectorService>().InstancePerLifetimeScope();
			//Service
			builder.RegisterType<LogService>().As<ILogService>().InstancePerLifetimeScope();


            builder.RegisterModule(new AutofacWebTypesModule());
            // register context
            RegisterEnrichContext(builder);
        }

        public void RegisterEnrichContext(ContainerBuilder builder)
        {
            builder.Register(c =>
            {
                if (HttpContext.Current == null)
                {
                    return new EnrichContext();
                }
                if (HttpContext.Current?.User.Identity.IsAuthenticated == true)
                {
                    string email = HttpContext.Current.User.Identity.Name;
                    var memRespo = EngineContext.Current.Resolve<IUnitOfWork>().Repository<P_Member>();
                    var mem = memRespo.TableNoTracking.Where(m => m.PersonalEmail.Equals(email) && m.Active == true).FirstOrDefault();
                    var accessToken = HttpContext.Current?.Session["ACCESS_TOKEN"] as string;
                    if (mem != null)
                    {
                        EnrichContext context = new EnrichContext()
                        {
                            Id = Guid.NewGuid().ToString(),
                            MemberId = mem.Id,
                            TrackTraceId = HttpContext.Current?.Session["traceTrackId"] as string,
                            MemberEmail = mem.PersonalEmail,
                            MemberFullName = mem.FullName,
                            IsProduction = Environment == Production,
                            AccessToken = accessToken
                        };
                        return context;
                    }
                    else
                    {
                        HttpContext.Current?.Session.Clear();
                        HttpContext.Current?.Session.Abandon();
                        HttpContext.Current?.Session.RemoveAll();
                        FormsAuthentication.SignOut();
                        HttpContext.Current?.Response.Redirect("/account/login");
                    }

                }

                return new EnrichContext();
            }).As<EnrichContext>().InstancePerRequest();
        }


        public int Order
        {
            get { return 0; }
        }
    }
}
