using Dapper;
using Enrich.Common.Helpers;
using Enrich.Core;
using Enrich.Dto;
using Enrich.Dto.Base.POSApi;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class NewCustomerGoalRepository : DapperGenericRepository<NewCustomerGoal>, INewCustomerGoalRepository
    {
        private readonly string Alias = SqlTables.NewCustomerGoal;
        private readonly EnrichContext _context;
        private readonly IEnrichLog _enrichLog;
        public NewCustomerGoalRepository(
            IConnectionFactory connectionFactory, 
            EnrichContext context, 
            IEnrichLog enrichLog)
           : base(connectionFactory)
        {
            _context = context;
            _enrichLog = enrichLog;
        }

        /// <summary>
        /// Check exist goal for time
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public async Task<bool> IsExistGoalForTime(int year, int month)
        {
            var query = SqlScript.NewCustomerGoal.IsExistGoalForTime(year, month, _context.SiteId);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstAsync<bool>(query);
            }
        }

        /// <summary>
        /// Get base serivce by store code
        /// </summary>
        /// <param name="key"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        public async Task<IEnumerable<NewCustomerGoal>> GetByYear(int year)
        {
            var query = SqlScript.NewCustomerGoal.GetByYear;
            var parameters = new DynamicParameters();
            parameters.Add(SqlScript.Parameters.Year, year);
            parameters.Add(SqlScript.Parameters.SiteId, _context.SiteId);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<NewCustomerGoal>(query, parameters);
            }
        }

        public async Task SaveByTranAsync(bool isNew, NewCustomerGoal goal)
        {
            using (_connectionFactory.SharedConnection = GetDbConnection())
            {
                if (_connectionFactory.SharedConnection.State == ConnectionState.Closed)
                    _connectionFactory.SharedConnection.Open();

                using (_connectionFactory.SharedTransaction = _connectionFactory.SharedConnection.BeginTransaction()) // begin transaction
                {
                    try
                    {
                        if (isNew)
                        {
                            var newId = await AddAsync(goal);
                            goal.Id = decimal.ToInt32((decimal)newId);
                        }
                        else
                        {
                            var affect = await UpdateAsync(goal);
                            if (affect == 0)
                                throw new Exception("Cannot save properties");
                        }
                        _connectionFactory.SharedTransaction.Commit();
                    }
                    catch (Exception)
                    {
                        _connectionFactory.SharedTransaction.Rollback();
                        throw;
                    }
                }
            }
        }

    }
}