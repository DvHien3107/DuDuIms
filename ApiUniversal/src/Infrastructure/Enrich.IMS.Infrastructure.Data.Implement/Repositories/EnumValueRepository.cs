using Dapper;
using Enrich.Common;
using Enrich.Dto.Base;
using Enrich.IMS.Dto.EnumValue;
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
    public class EnumValueRepository : DapperGenericRepository<EnumValue>, IEnumValueRepository
    {
        public EnumValueRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }

        public async Task<List<IdNameDto>> GetEnumValuesAsync(string nameSpace, params string[] enumValues)
        {
            var query = $"SELECT Value, Text, Id FROM dbo.EnumValue WHERE Namespace=@NameSpace";
            if (!enumValues.IsNullOrEmpty()) query += $" AND Value IN ({enumValues.ToStringList()})";
            query += " ORDER BY Order";

            IEnumerable<EnumValue> entities;

            using (var db = GetDbConnection())
            {
                entities = await db.QueryAsync<EnumValue>(query, new { Namespace = nameSpace });
            }

            return ParseIdNames(entities);
        }

        public override async Task<IEnumerable<IdNameDto>> GetIdNamesAsync(LookupDataRequest request)
        {
            var conditions = new List<string>();
            var param = new Dictionary<string, object>();

            // find by searchtext
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {

                conditions.Add($"Value IN (SELECT search.EnumValue FROM dbo.EnumValue search WHERE Namespace=@NameSpace  search.EnumText LIKE @SeachText)");
                param.Add("SeachText", $"%{request.SearchText}%");
            }

            // find by ids
            if (request.InIds?.Length > 0)
            {
                conditions.Add($"EnumValue IN ({request.InIds.ToStringList()})");
            }

            // find by namespace
            if (request is EnumValueLookupDataRequest frequest)
            {
                if (!string.IsNullOrWhiteSpace(frequest.Namespace))
                {
                    conditions.Add("Namespace=@NameSpace");
                    param.Add("Namespace", frequest.Namespace);
                }
            }

            if (conditions.Count == 0)
            {
                return Enumerable.Empty<IdNameDto>();
            }

            var query = $@"SELECT [id], [Value], [Name] FROM dbo.EnumValue
                        WHERE 1=1
                        AND {conditions.ToStringList(" AND ")}
                        ORDER BY [Order]";

            var dbEnumValues = new List<EnumValue>();

            using (var db = GetDbConnection())
            {
                dbEnumValues.AddRange(await db.QueryAsync<EnumValue>(query, param));
            }

            return ParseIdNames(dbEnumValues);
        }

        #region Methods

        private List<IdNameDto> ParseIdNames(IEnumerable<EnumValue> enumValues)
        {
            var idNames = new List<IdNameDto>();

            foreach (var item in enumValues)
            {
                var idName = new IdNameDto()
                {
                    //Id = item.Id,
                    Name = item.Name,
                    Value = item.Value,
                    IconUrl = item.IconUrl
                };

                idNames.Add(idName);
            }

            return idNames;
        }

        #endregion
    }
}
