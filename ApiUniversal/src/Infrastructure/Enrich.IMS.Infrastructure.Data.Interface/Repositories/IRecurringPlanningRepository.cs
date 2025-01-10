using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface IRecurringPlanningRepository : IGenericRepository<RecurringPlanning>
    {
        public Task<IEnumerable<RecurringPlanning>> GetByRecurringDate();

        Task<IEnumerable<RecurringPlanningDto>> GetByCustomerCodeAsync(string customerCode);
    }
}