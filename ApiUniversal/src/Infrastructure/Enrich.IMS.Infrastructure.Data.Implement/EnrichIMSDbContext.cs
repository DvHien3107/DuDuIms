using Dapper.FluentMap;
using Dapper.FluentMap.Dommel;
using Enrich.IMS.Infrastructure.Data.Implement;
using Enrich.IMS.Infrastructure.Data.Implement.TableMapper;

namespace Enrich.Infrastructure.Data
{
    public class EnrichIMSDbContext
    {
        public EnrichIMSDbContext()
        {
            FluentMapper.Initialize(config =>
            {
                config.AddMap(new MemberEntityMapper());
                config.AddMap(new AccessKeyEntityMapper());
                config.AddMap(new TicketEntityMapper());
                config.AddMap(new TicketStatusEntityMapper());
                config.AddMap(new TicketTypeEntityMapper());
                config.AddMap(new TicketTypeMappingEntityMapper());
                config.AddMap(new TicketStatusMappingEntityMapper());
                config.AddMap(new EmailTemplateEntityMapper());
                config.AddMap(new OrderEntityMapper());
                config.AddMap(new OrderEventEntityMapper());
                config.AddMap(new BusinessEventEntityMapper());
                config.AddMap(new OrderSubscriptionEntityMapper());
                config.AddMap(new CustomerEntityMapper());
                config.AddMap(new SalesLeadEntityMapper());
                config.AddMap(new SalesLeadCommentEntityMapper());
                config.AddMap(new CalendarEventEntityMapper());
                config.AddMap(new CustomerTransactionEntityMapper());
                config.AddMap(new SystemConfigurationEntityMapper());
                config.AddMap(new CustomerCardEntityMapper());
                config.AddMap(new StoreServiceEntityMapper());
                config.AddMap(new PartnerEntityMapper());
                config.AddMap(new LicenseItemEntityMapper());
                config.AddMap(new LicenseProductEntityMapper());
                config.AddMap(new CustomerGiftcardEntityMapper());
                config.AddMap(new RecurringPlanningEntityMapper());
                config.AddMap(new RecurringHistoryEntityMapper());
                config.AddMap(new OptionConfigEntityMapper());
                config.AddMap(new DepartmentMapper());
                config.AddMap(new SalesLeadInteractionStatusEntityMapper());
                config.AddMap(new StoreBaseServiceEntityMapper());
                config.AddMap(new NewCustomerGoalEntityMapper());
                config.AddMap(new TicketFeedbackEntityMapper());
                config.AddMap(new ProjectMilestoneEntityMapper());
                config.ForDommel();
            });
        }
    }
}
