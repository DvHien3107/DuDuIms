using Pos.Application.DBContext;
using Pos.Application.Event;
using Pos.Application.Event.POS;
using Pos.Application.Repository;
using Pos.Application.Repository.IMS;
using Pos.Application.Services.Scoped;
using Pos.Application.Services.Scoped.IMS;
using Pos.Application.Services.Scoped.Payment;
using Pos.Application.Services.Singleton;

namespace PosAPI.Builder
{
    public static class StartUpBuilder
    {
        public static void Build( IServiceCollection Services) {


            Services.AddScoped<ILicenseEvent, LicenseEvent>();
            Services.AddScoped<IOrderEvent, OrderEvent>();
            Services.AddScoped<ICustomerEvent, CustomerEvent>();
            Services.AddScoped<IOrderRepository, OrderRepository>();
            Services.AddScoped<IMemberRespository, MemberRespository>();
            Services.AddScoped<ICustomerRespository, CustomerRespository>(); 
            Services.AddScoped<IRecurringRepository, RecurringRepository>();

            Services.AddScoped<IRCPService, RCPService>();
            Services.AddScoped<ILoginService, LoginService>();
            Services.AddScoped<IPOSService, POSService>();
            Services.AddScoped<IIMSService, IMSService>();
            Services.AddScoped<ICustomerService, CustomerService>();
            Services.AddScoped<IEmailService, EmailService>();
            Services.AddScoped<ILicenseService, LicenseService>();
            Services.AddScoped<IOrderService, OrderService>();
            Services.AddScoped<IAuthorizeNetService, AuthorizeNetService>();
            Services.AddScoped<IRecurrringService, RecurrringService>();
            Services.AddScoped<ITransactionRepository, TransactionRepository>();
            Services.AddScoped<ITransactionService, TransactionService>();

            Services.AddScoped<IStoreServiceRepository, StoreServiceRepository>();
            Services.AddScoped<IStoreService, StoreService>();
            Services.AddScoped<IMemberService, MemberService>();

            Services.AddScoped<ITicketRepository, TicketRepository>();
            Services.AddScoped<ITicketService, TicketService>();

            Services.AddSingleton<IClanService, ClanService>();
            Services.AddSingleton<ITwilioService, TwilioService>();
            Services.AddSingleton<IIMSRequestService, IMSRequestService>();
            Services.AddSingleton<IEmailRepository, EmailRepository>();
            Services.AddSingleton<ILoginRepository, LoginRepository>();
            Services.AddSingleton<IRcpRepository, RcpRepository>();


        }
    }
}
