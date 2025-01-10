using Dapper;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public class RecurringHistoryRepository : DapperGenericRepository<RecurringHistory>, IRecurringHistoryRepository
    {
        public RecurringHistoryRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }
    }
}