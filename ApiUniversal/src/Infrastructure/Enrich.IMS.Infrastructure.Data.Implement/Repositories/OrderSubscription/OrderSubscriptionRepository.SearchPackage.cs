using Dapper;
using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto.Base.Parameters;
using Enrich.Dto.List;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.OrderSubscription;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class OrderSubscriptionRepository
    {
        public async Task<SubscriptionPackageSearchResponse> SearchSubscriptionPackageAsync(SubscriptionPackageSearchRequest request)
        {
            var response = new SubscriptionPackageSearchResponse();
            string package = string.Empty;
            string storeCodes = "''";

            if (request.Condition.StoreCodes != null) storeCodes = CommonHelper.ToStringListWithSpecial(request.Condition.StoreCodes);
            if (!string.IsNullOrEmpty(request.Condition.Package)) package = request.Condition.Package;

            var query = SqlScript.OrderSubscription.SearchTransactionPackage(package, storeCodes, request.Condition.FromDate, request.Condition.ToDate);
            using (var connection = GetDbConnection())
            {
                 response.Records = await connection.QueryAsync<SubscriptionPackageListItemDto>(query);
            }

            return response;
        }
    }
}
