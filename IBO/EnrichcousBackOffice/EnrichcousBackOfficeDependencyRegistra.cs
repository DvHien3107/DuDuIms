using Autofac;
using Enrich.Core.Infrastructure;
using Enrich.Core.Infrastructure.DependencyManagement;
using EnrichcousBackOffice.Services.NextGen.Mail;
using EnrichcousBackOffice.Services.Repository;
using EnrichcousBackOffice.ViewControler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice
{
    public class EnrichcousBackOfficeDependencyRegistra : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<CalendarViewService>().InstancePerLifetimeScope();
            builder.RegisterType<ContractViewService>().InstancePerLifetimeScope();
            builder.RegisterType<EmployeesViewService>().InstancePerLifetimeScope();
            builder.RegisterType<InventoryViewService>().InstancePerLifetimeScope();
            builder.RegisterType<QuestNonTechViewService>().InstancePerLifetimeScope();
            builder.RegisterType<SaleLeadViewService>().InstancePerLifetimeScope();
            builder.RegisterType<StoreViewService>().InstancePerLifetimeScope();
            builder.RegisterType<TaskViewService>().InstancePerLifetimeScope();
            builder.RegisterType<TicketViewService>().InstancePerLifetimeScope();


            builder.RegisterType<CustomerRepository>().As<ICustomerRepository>().InstancePerLifetimeScope();
            builder.RegisterType<OrderRepository>().As<IOrderRepository>().InstancePerLifetimeScope();
            builder.RegisterType<EmailService>().As<IEmailService>().InstancePerLifetimeScope();

        }

        public int Order
        {
            get { return 2; }
        }
    }
}