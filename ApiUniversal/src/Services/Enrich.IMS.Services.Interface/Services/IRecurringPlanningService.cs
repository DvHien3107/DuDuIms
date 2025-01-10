using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface IRecurringPlanningService : IGenericService<RecurringPlanning, RecurringPlanningDto>
    {
         Task<IEnumerable<RecurringPlanning>> GetByRecurringDate();
         Task<Order> RecurringSubsciptionActionAsync(List<RecurringPlanning> recurringPlannings);
         Task RecurringSubscriptionAsync();
         Task<string> RecurringSubscriptionAsync_NextGen();
         Task RecurringSubscriptionByCustomerCodeAsync(string customerCode);
        Task DoProc(string query);

    }
}
