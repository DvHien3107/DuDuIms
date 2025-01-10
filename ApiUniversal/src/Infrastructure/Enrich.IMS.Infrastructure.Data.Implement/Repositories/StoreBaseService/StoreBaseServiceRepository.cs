using Dapper;
using Enrich.Common.Helpers;
using Enrich.Core;
using Enrich.Dto;
using Enrich.Dto.Base.POSApi;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class StoreBaseServiceRepository : DapperGenericRepository<StoreBaseService>, IStoreBaseServiceRepository
    {
        private readonly EnrichContext _context;
        private readonly IEnrichLog _enrichLog;
        public StoreBaseServiceRepository(
            IConnectionFactory connectionFactory, 
            EnrichContext context, 
            IEnrichLog enrichLog)
           : base(connectionFactory)
        {
            _context = context;
            _enrichLog = enrichLog;
        }

        /// <summary>
        /// Get base serivce by store code
        /// </summary>
        /// <param name="key"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        public async Task<StoreBaseService> GetBaseServiceByStoreCode(string key, string store)
        {
            var query = SqlScript.StoreBaseService.GetRemaining;
            var parameters = new DynamicParameters();
            parameters.Add(SqlScript.StoreBaseService.Key.KeyName, key);
            parameters.Add(SqlScript.StoreBaseService.Key.StoreCode, store);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<StoreBaseService>(query, parameters);
            }
        }

    }
}