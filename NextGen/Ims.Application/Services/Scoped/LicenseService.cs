using Enrich.IMS.Entities;
using Pos.Application.Extensions.Helper;
using Pos.Model.Model.Table.IMS;
using Promotion.Application.Extensions;
using Promotion.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Services.Scoped
{
    public interface ILicenseService : IEntityService<TwilioAccount>
    {
        Task<List<RecurringPlanning>> getToDayRecurringPlan();
    }
    public class LicenseService : IMSEntityService<TwilioAccount>, ILicenseService
    {
        public async Task<List<RecurringPlanning>> getToDayRecurringPlan()
        {
            List<RecurringPlanning> result = (await _connection.SqlQueryAsync<RecurringPlanning>("exec P_GetToDayRecurringPlan")).ToList();
            return result;
        }
    }
}
