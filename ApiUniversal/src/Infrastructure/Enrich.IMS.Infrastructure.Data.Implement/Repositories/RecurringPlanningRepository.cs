using Dapper;
using Enrich.Common.Enums;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public class RecurringPlanningRepository : DapperGenericRepository<RecurringPlanning>, IRecurringPlanningRepository
    {
        public RecurringPlanningRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }
        public async Task<IEnumerable<RecurringPlanning>> GetByRecurringDate()
        {
            var query = @"SELECT * FROM " + SqlTables.RecurringPlanning + @" as rp WITH (NOLOCK) WHERE FORMAT(RecurringDate, 'yyyy-MM-dd') = FORMAT(GETUTCDATE(), 'yyyy-MM-dd') AND Status = @status AND
                        (rp.SubscriptionType = 'license' OR (
	                    rp.SubscriptionType != 'license' AND
	                    (SELECT count(*) FROM Store_Services as ss WITH (NOLOCK) 
	                    WHERE ss.[Active] = 1 AND ss.[Type] = 'license' AND ss.[CustomerCode] = rp.CustomerCode AND CAST(GETUTCDATE() AS DATE) <= CAST(ss.[RenewDate] AS DATE)) > 0
	                    ))";
            var parameters = new DynamicParameters();
            parameters.Add("status", (int)RecurringEnum.PlanStatus.Enable);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<RecurringPlanning>(query, parameters);
            }
        }

        public async Task<IEnumerable<RecurringPlanning>> GetByCustomerCode(string customerCode)
        {
            var query = @"SELECT * FROM " + SqlTables.RecurringPlanning + @" as rp WITH (NOLOCK) WHERE FORMAT(RecurringDate, 'yyyy-MM-dd') = FORMAT(GETUTCDATE(), 'yyyy-MM-dd') AND Status = @status AND
                        (rp.SubscriptionType = 'license' OR (
	                    rp.SubscriptionType != 'license' AND
	                    (SELECT count(*) FROM Store_Services as ss WITH (NOLOCK) 
	                    WHERE ss.[Active] = 1 AND ss.[Type] = 'license' AND ss.[CustomerCode] = rp.CustomerCode AND CAST(GETUTCDATE() AS DATE) <= CAST(ss.[RenewDate] AS DATE)) > 0
	                    ))";
            var parameters = new DynamicParameters();
            parameters.Add("status", (int)RecurringEnum.PlanStatus.Enable);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<RecurringPlanning>(query, parameters);
            }
        }

        public async Task<IEnumerable<RecurringPlanningDto>> GetByCustomerCodeAsync(string customerCode)
        {
            var query = SqlScript.RecurringPlanning.GetByCustomerCode;
            var parameters = new DynamicParameters();
            parameters.Add(SqlScript.Parameters.CustomerCode, customerCode);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<RecurringPlanningDto>(query, parameters);
            }
        }
    }
}