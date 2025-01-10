using Dapper;
using Enrich.Common;
using Enrich.Dto.Base;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Lookup;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public class WebGridConfigRepository : DapperGenericRepository<WebGridConfig>, IWebGridConfigRepository
    {
        // GridOwnerType
        // - GlobalUser: UserId = 0 (null)
        // - User: UserId > 0

        public WebGridConfigRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }


        public async Task<WebGridConfig> GetUserGridConfigAsync(GridType type, long userId, bool includeGlobal = true)
        {
            //var filterUser = $"UserId = {userId} ";
            //if (includeGlobal)
            //{
            //    filterUser = $"(({filterUser}) OR UserId = 0 OR UserId IS NULL)";
            //}

            var query = $"SELECT TOP 1 * FROM WebGridConfig WHERE [Type] = {(int)type}";

            return await FirstOrDefaultAsync<WebGridConfig>(query);
        }

        public async Task<int> SaveGridConfigAsync(SaveWebGridConfigRequest request)
        {
            //IMPORTANT: always make sure data is validation in service/controller
            var query = $@"DECLARE @ConfigId INT
SELECT TOP 1 @ConfigId = Id FROM dbo.WebGridConfigs WHERE [Type] = @Type AND #CONDITION#
IF ISNULL(@ConfigId, 0) > 0
  UPDATE WebGridConfigs SET ConfigAsJson = @ConfigAsJson WHERE Id = @ConfigId
ELSE BEGIN
  INSERT WebGridConfigs([Type], UserId, ConfigAsJson) VALUES(@Type, @UserId, @ConfigAsJson)
  SET @ConfigId = SCOPE_IDENTITY();
END
SELECT @ConfigId";

            //process
            var filterCondition = string.Empty;
            var deleteCondition = string.Empty;

            var parameters = new DynamicParameters(new
            {
                Type = (int)request.Type,
                request.ConfigAsJson
            });

            switch (request.OwnerType.Value)
            {
                case GridOwnerType.User:
                    filterCondition = "UserId = @UserId ";
                    parameters.Add("UserId", request.UserId);
                    break;
              
                case GridOwnerType.GlobalUser:
                    filterCondition = "(UserId = 0 OR UserId IS NULL) ";
                    deleteCondition = $"{(request.UserIds.IsNullOrEmpty() ? "UserId > 0" : $"UserId IN ({request.UserIds.ToStringList()})")} ";
                    parameters.Add("UserId", 0);
                    break;
                default:
                    return -1;
            }

            if (!string.IsNullOrWhiteSpace(deleteCondition))
                query = $@"--delete used, update global
DELETE WebGridConfigs WHERE [Type]=@Type AND {deleteCondition}{NewLine}{query}";

            query = query.Replace("#CONDITION#", filterCondition);

            using (var db = GetDbConnection())
            {
                var configId = await db.ExecuteScalarAsync<int>(query, parameters);
                return configId;
            }
        }
    }
}
