using Dapper;
using Dommel;
using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto;
using Enrich.Dto.Base;
using Enrich.Dto.Base.Parameters;
using Enrich.Dto.List;
using Enrich.Dto.Parameters;
using Enrich.IMS.Dto.Common;
using Enrich.IMS.Dto.Customer;
using Enrich.Infrastructure.Data.Dapper.Library;
using SqlKata;
using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Enrich.Infrastructure.Data.Dapper
{
    public partial class DapperGenericRepository<T>
    {
        SqlServerCompiler _sqlCompiler = new SqlServerCompiler();

        string OFFSET_SQL = $@"WHERE [row_num] BETWEEN #OFFSETFROM AND #OFFSETTO";
        string ORDER_BY_SQL = $@"ORDER BY #ORDERBY";

        public async Task<PagingResponseDto<TDto>> QueryPagingAsync<TDto>(QueryParameter parameter)
        {
            DynamicParameters sqlParam = null;

            string query = GenerateQuerySql(parameter, out sqlParam);
            string queryCount = GenerateQueryCountSql(parameter);

            string multiQuery = $@"{query}{Environment.NewLine}{queryCount}";

            using (var connection = GetDbConnection())
            {
                using (var multi = await connection.QueryMultipleAsync(multiQuery))
                {
                    IEnumerable<TDto> records = await multi.ReadAsync<TDto>();
                    int totalRecords = await multi.ReadSingleAsync<int>();
                    int pageCount = CalculatePageCount(totalRecords, parameter.Paging);
                    int pageSize = CalculatePageSize(totalRecords, parameter.Paging, pageCount);

                    return new PagingResponseDto<TDto>()
                    {
                        Records = records,
                        Pagination = new PaginationDto()
                        {
                            TotalRecords = totalRecords,
                            PageIndex = parameter.Paging.PageIndex,
                            PageCount = pageCount,
                            PageSize = pageSize
                        }
                    };
                }
            }
        }

        public TDto FindById<TDto>(FindByIdParameter queryParameter)
        {
            DynamicParameters sqlParameters;
            var sql = BuildFindByIdSql(typeof(T), queryParameter, out sqlParameters);
            using (var connection = GetDbConnection())
            {
                return connection.QueryFirstOrDefault<TDto>(sql, sqlParameters);
            }
        }

        public TDto FindById<TDto>(object Id)
        {
            DynamicParameters sqlParameters;
            var sql = BuildFindByIdSql(typeof(TDto), Id, out sqlParameters);
            using (var connection = GetDbConnection())
            {
                return connection.QueryFirstOrDefault<TDto>(sql, sqlParameters);
            }
        }

        public async Task<IEnumerable<TDto>> FindByIds<TDto>(int[] Ids)
        {
            DynamicParameters sqlParameters;
            var sql = BuildFindByIdsSql(typeof(TDto), Ids, out sqlParameters);
            try
            {
                using (var connection = GetDbConnection())
                {
                    return await connection.QueryAsync<TDto>(sql, sqlParameters);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        string BuildFindByIdSql(Type entityType, FindByIdParameter parameter, out DynamicParameters parameters)
        {
            string sql;
            var keyProperty = Resolvers.KeyProperties(entityType);
            var keyColumnName = Resolvers.Column(keyProperty.FirstOrDefault().Property, null);

            string fieldSelectClause = GenerateFieldSelectClause(parameter.Fields);

            sql = $"SELECT {fieldSelectClause} FROM {TableName} WHERE {keyColumnName} = @Id";

            parameters = new DynamicParameters();
            parameters.Add("Id", parameter.Id);

            return sql;
        }

        string BuildFindByIdSql(Type entityType, object id, out DynamicParameters parameters)
        {
            string sql;
            var keyProperty = Resolvers.KeyProperties((typeof(T))).FirstOrDefault().Property;
            var keyColumnName = Resolvers.Column(keyProperty, null);
            var dbFields = FieldDbHelper.GetFields(entityType);
            sql = $"SELECT {dbFields.Select(a => $"{a.Name} as [{a.Prop.Name}]").ToStringList()} FROM {TableName} WHERE {keyColumnName} = @Id";

            parameters = new DynamicParameters();
            parameters.Add("Id", id);

            return sql;
        }

        string BuildFindByIdsSql(Type entityType, int[] ids, out DynamicParameters parameters)
        {
            string sql;
            var keyProperty = Resolvers.KeyProperties((typeof(T))).FirstOrDefault().Property;
            var keyColumnName = Resolvers.Column(keyProperty, null);
            var dbFields = FieldDbHelper.GetFields(entityType);
            sql = $"SELECT {dbFields.Select(a => $"{a.Name} as [{a.Prop.Name}]").ToStringList()} FROM {TableName} WHERE {keyColumnName} in @Ids ";

            parameters = new DynamicParameters();
            parameters.Add("@Ids", ids);

            return sql;
        }

        protected int CalculatePageCount(int totalRecords, PagingParameter paging)
        {
            return (int)(Math.Ceiling(totalRecords / (double)paging.PageSize));
        }

        protected int CalculatePageSize(int totalRecords, PagingParameter paging, int? pageCount = null)
        {
            int currentPageSize = paging.PageSize;
            int totalPage = pageCount ?? CalculatePageCount(totalRecords, paging);
            if (paging.PageIndex < totalPage)
                currentPageSize = paging.PageSize;
            else//last page
            {
                if (paging.PageIndex == totalPage)
                    currentPageSize = totalRecords % paging.PageSize;
            }

            if (currentPageSize == 0)
                currentPageSize = paging.PageSize;

            return currentPageSize;
        }

        protected virtual string GenerateQuerySqlOld(QueryParameter parameter)
        {
            var query = new Query(TableName);
            if (parameter.OrderBy != null)
            {
                foreach (var orderByField in parameter.OrderBy)
                {
                    if (orderByField.Direction == SortDirectionEnum.Descending)
                        query.OrderByDesc(orderByField.Field.SqlName);
                    else
                        query.OrderBy(orderByField.Field.SqlName);
                }
            }
            if (parameter.Paging != null)
            {
                //int offset = (parameter.Paging.PageIndex - 1) * parameter.Paging.PageSize;
                //query.Limit(parameter.Paging.PageSize).Offset(offset);
                query.ForPage(parameter.Paging.PageIndex, parameter.Paging.PageSize);
            }


            if (parameter.Fields != null)
            {
                foreach (var selectField in parameter.Fields)
                {
                    query.Select($@"{selectField.SqlName} AS {selectField.DtoName}");
                }
            }

            var abc = _sqlCompiler.Compile(query);
            string sql = abc.ToString();
            //temporary fix sqlkata, we need upgrade to latest version to help resolve
            sql = sql.Replace($@"WHERE [row_num]", "as #temp WHERE [row_num]");
            return sql;
        }

        protected virtual string GenerateQuerySql(QueryParameter parameter, out DynamicParameters sqlParam)
        {
            sqlParam = new DynamicParameters();
            DynamicParameters whereParameters = null;
            string templateQuerySql = GetTemplateQuerySql();
            var querySql = templateQuerySql.Replace("#TABLENAME", TableName);

            string fieldClause = GenerateFieldSelectClause(parameter.Fields);
            string whereClause = GenerateQueryWhereClause(parameter, out whereParameters);
            if (whereParameters != null)
                sqlParam.AddDynamicParams(whereParameters);

            string offsetClause = GenerateOffsetClause(parameter);
            string orderByClause = GenerateOrderByClause(parameter);

            querySql = querySql.Replace("#FIELDS", fieldClause);
            querySql = querySql.Replace("#WHERE", whereClause);
            querySql = querySql.Replace("#ORDERBY", orderByClause);
            querySql = querySql.Replace("#OFFSET", offsetClause);

            return querySql;
        }

        protected virtual string GenerateQueryCountSql(QueryParameter parameter)
        {
            DynamicParameters whereParameters = null;
            string templateQuerySql = GetTemplateQueryCountSql();
            var queryCountSql = templateQuerySql.Replace("#TABLENAME", TableName);

            string whereClause = GenerateQueryWhereClause(parameter, out whereParameters);
            queryCountSql = queryCountSql.Replace("#WHERE", whereClause);

            return queryCountSql;
        }

        protected string GenerateFieldSelectClause(IEnumerable<SqlMapDto> fields)
        {
            if (fields == null || fields.Count() == 0)
                return "*";//select *
            List<string> lstFields = new List<string>();
            foreach (var columnMapper in fields)
            {
                lstFields.Add($@"{TableName}.[{columnMapper.SqlName}] as [{columnMapper.DtoName}]");
            }
            return string.Join(",", lstFields.ToArray());
        }

        protected virtual string GenerateQueryWhereClause(QueryParameter parameter, out DynamicParameters whereParameters)
        {
            whereParameters = new DynamicParameters();

            return "(1=1)";
        }

        public string GenerateOffsetClause(QueryParameter parameter)
        {
            string sql = string.Empty;
            if (parameter != null && parameter.Paging != null)
            {
                var offset = GetPagingOffset(parameter.Paging);

                sql = OFFSET_SQL.Replace("#OFFSETFROM", offset.From.ToString()).Replace("#OFFSETTO", offset.To.ToString());
            }
            return sql;
        }

        public string GenerateOrderByClause(QueryParameter parameter)
        {
            var sql = $@"ORDER BY (SELECT 0)"; //just default

            if (parameter != null && parameter.OrderBy != null && parameter.OrderBy.Count() > 0)
            {
                var sorts = new List<string>();

                foreach (var orderByField in parameter.OrderBy)
                {
                    if (orderByField.Direction == SortDirectionEnum.Ascending)
                        sorts.Add($@"{TableName}.{orderByField.Field.SqlName} ASC");

                    else if (orderByField.Direction == SortDirectionEnum.Descending)
                        sorts.Add($@"{TableName}.{orderByField.Field.SqlName} DESC");
                }

                sql = ORDER_BY_SQL.Replace("#ORDERBY", string.Join(",", sorts.ToArray()));
            }

            return sql;
        }

        protected virtual string GetTemplateQuerySql()
        {
            return $@"SELECT * FROM 
                                        (SELECT #FIELDS, 
                                            ROW_NUMBER() OVER (#ORDERBY) AS [row_num] 
                                            FROM #TABLENAME
                                            WHERE #WHERE
                                        ) as #temp 
                                        #OFFSET";
        }

        protected virtual string GetTemplateQueryCountSql()
        {
            return $@"SELECT COUNT(1) 
                            FROM #TABLENAME
                            WHERE #WHERE";
        }

        protected async Task<PagingResponseDto<TItem>> ExecuteQueryAsync<TItem>(string query, DynamicParameters parameters, QueryParameter queryParam)
        {
            var response = new PagingResponseDto<TItem>();

            using (var connection = GetDbConnection())
            {
                response.Records = await connection.QueryAsync<TItem>(query, parameters);
                response.Pagination = new PaginationDto { TotalRecords = response.Records.Count() };

                if (queryParam.HasPaging)
                {
                    var totalRecords = response.Records == null ? 0 : (response.Records.FirstOrDefault() as ListItemDto)?.TotalRows ?? 0;
                    response.Pagination = PopulatePagination(totalRecords, queryParam.Paging);
                }
            }

            return response;
        }

        protected async Task<PagingResponseDto<TItem>> ExecuteQueryByTemplateAsync<TItem>(string queryTemplate, QueryParameter queryParam, List<string> extendJoins, List<string> conditions, DynamicParameters parameters = null) where TItem : ListItemDto
        {
            var sqlInfo = ParseSqlQueryInfoByTemplate(queryTemplate, queryParam, extendJoins, conditions);

            return await ExecuteQueryAsync<TItem>(sqlInfo, queryParam.Paging, parameters);
        }

        protected async Task<PagingResponseDto<TItem>> ExecuteQueryAsync<TItem>(ParsedSqlQuery sqlInfo, PagingParameter paging, DynamicParameters parameters = null, Action<GridReader> extendReader = null) where TItem : ListItemDto
        {
            var response = new PagingResponseDto<TItem>();

            using (var connection = GetDbConnection())
            {
                //new way
                var queryExecution = sqlInfo.QueryFinally;

                if (sqlInfo.HasPaging)
                {
                    var offset = GetPagingOffset(paging);

                    queryExecution = $@";WITH cteMain AS (
    {sqlInfo.QueryNoRowNumber}
)
SELECT * FROM (
  SELECT ROW_NUMBER() OVER ({GenerateSqlOrderBy(sqlInfo.QueryParameter.OrderBy, useDtoName: true)}) AS [__row_num], COUNT(*) OVER() AS {nameof(ListItemDto.TotalRows)}, *
  FROM cteMain
) tbl
WHERE [__row_num] BETWEEN {offset.From} AND {offset.To}";
                }

                //extend
                queryExecution = sqlInfo.PopulateQueryExtend(queryExecution);

                //execute
                using (var reader = await connection.QueryMultipleAsync(queryExecution, parameters))
                {
                    response.Records = await reader.ReadAsync<TItem>();

                    if (sqlInfo.HasPaging)
                    {
                        var totalRecords = response.Records != null ? (response.Records.FirstOrDefault() as ListItemDto)?.TotalRows ?? 0 : 0;
                        response.Pagination = PopulatePagination(totalRecords, paging);
                    }
                    else
                    {
                        response.Pagination = new PaginationDto { TotalRecords = response.Records.Count() };
                    }

                    extendReader?.Invoke(reader);
                }

                //old way
                //var queryFinal = $"{sqlInfo.QueryFinal}{NewLine}{sqlInfo.QueryExtend}";

                //using (var reader = await connection.QueryMultipleAsync(queryFinal, parameters))
                //{
                //    response.Records = await reader.ReadAsync<TItem>();

                //    if (sqlInfo.HasPaging)
                //    {
                //        var totalRecords = await reader.ReadFirstAsync<int>();
                //        response.Pagination = PopulatePagination(totalRecords, paging);
                //    }
                //    else
                //    {
                //        response.Pagination = new PaginationDto { TotalRecords = response.Records.Count() };
                //    }

                //    extendReader?.Invoke(reader);
                //}
            }

            return response;
        }

        protected async Task<List<CustomerListItemDto>> ExecuteQuerySearchMechant(string SqlQuery) 
        {
            using (var connection = GetDbConnection())
            {
                //execute
                var reader = await connection.QueryAsync<CustomerListItemDto>(SqlQuery);
                return reader.ToList();
            }
        }

        protected async Task<PagingResponseDto<TItem>> ExecuteQueryCTEAsync<TItem>(ParsedSqlQuery sqlInfo, PagingParameter paging, DynamicParameters parameters = null, Action<GridReader> extendReader = null) where TItem : ListItemDto
        {
            var response = new PagingResponseDto<TItem>();

            using (var connection = GetDbConnection())
            {
                //new way
                var queryExecution = sqlInfo.QueryFinally;

                //extend
                queryExecution = sqlInfo.PopulateQueryExtend(queryExecution);

                //execute
                using (var reader = await connection.QueryMultipleAsync(queryExecution, parameters))
                {
                    response.Records = await reader.ReadAsync<TItem>();

                    if (sqlInfo.HasPaging)
                    {
                        var totalRecords = (await reader.ReadAsync<int>()).FirstOrDefault();
                        response.Pagination = PopulatePagination(totalRecords, paging);
                    }
                    else
                    {
                        response.Pagination = new PaginationDto { TotalRecords = response.Records.Count() };
                    }

                    extendReader?.Invoke(reader);
                }

            }

            return response;
        }

        protected PaginationDto PopulatePagination(int totalRecords, PagingParameter paging)
        {
            var pageCount = CalculatePageCount(totalRecords, paging);
            var pageSize = CalculatePageSize(totalRecords, paging, pageCount);

            return new PaginationDto
            {
                TotalRecords = totalRecords,
                PageCount = pageCount,
                PageIndex = paging.PageIndex,
                PageSize = pageSize
            };
        }

        protected ParsedSqlQuery ParseSqlQueryInfoByTemplate(string queryTemplate, QueryParameter queryParam, List<string> extendJoins, List<string> conditions, string aliasFieldRowNumber = "__row_num")
        {
            var info = new ParsedSqlQuery(queryParam) { HasPaging = queryParam.HasPaging };

            var hasPaging = queryParam.HasPaging;
            var queryMain = queryTemplate;

            info.StatementFromAndWhere = queryTemplate.Replace("#SELECT#", string.Empty).Replace("#ORDERBY#", string.Empty); ;

            //extend joins
            var queryExtendJoins = extendJoins != null ? string.Join(Environment.NewLine, extendJoins) : string.Empty;
            queryMain = queryMain.Replace("#EXTENDJOIN#", queryExtendJoins).Trim();
            info.StatementFromAndWhere = info.StatementFromAndWhere.Replace("#EXTENDJOIN#", queryExtendJoins).Trim();

            //order by
            info.StatementOrderBy = GenerateSqlOrderBy(queryParam.OrderBy);
            queryMain = queryMain.Replace("#ORDERBY#", info.HasPaging ? string.Empty : info.StatementOrderBy); //if paging, order by clause will be in field row numer

            //where
            if (conditions != null) conditions.RemoveAll(string.IsNullOrWhiteSpace);
            var filterCondition = conditions?.Count > 0 ? $" AND {string.Join($"{Environment.NewLine}AND ", conditions)}" : string.Empty;

            queryMain = queryMain.Replace("#CONDITION#", filterCondition);
            info.StatementFromAndWhere = info.StatementFromAndWhere.Replace("#CONDITION#", filterCondition).Trim();

            //check: if order-fields not in select-fields -> include order-fields to select-fields
            if (queryParam.OrderBy?.Count > 0)
            {
                foreach (var item in queryParam.OrderBy.Where(a => queryParam.Fields.All(b => a.Field.DtoName != b.DtoName)))
                {
                    queryParam.Fields.Add(item.Field);
                }
            }

            //select
            var selectFields = GenerateSqlSelect(queryParam.Fields, includeKeywordSelect: false, includeKeyworkDistinct: false);
            var statementSelectNoRowNumber = $"SELECT DISTINCT {selectFields}";

            if (info.HasPaging)
                selectFields = $"{GenerateFieldRowNumber(info.StatementOrderBy, aliasFieldRowNumber, operation: "DENSE_RANK")},{NewLine}{selectFields}";

            info.StatementSelect = $"SELECT DISTINCT {selectFields}";

            info.QueryNoRowNumber = queryMain.Replace("#SELECT#", statementSelectNoRowNumber).Trim();

            //final query
            info.Query = info.QueryFinally = queryMain.Replace("#SELECT#", info.StatementSelect).Trim();

            if (info.HasPaging)
            {
                var countDistinceField = queryParam.Fields.FirstOrDefault(a => a.DtoName.EqualsEx("Id"));

                info.QueryPaging = GenerateSqlQueryPaging(info.Query, queryParam.Paging, aliasFieldRowNumber: aliasFieldRowNumber).Trim();
                info.QueryTotalRecords = GenerateSqlQueryTotalRecords(queryTemplate, filterCondition, queryExtendJoins, countDistinceField).Trim();
                info.QueryFinally = $"{info.QueryPaging}{Environment.NewLine}--Query TotalRecords{Environment.NewLine}{info.QueryTotalRecords}";
            }

            return info;
        }

        /// <summary>
        /// use for filter by column 
        /// </summary>
        /// <returns></returns>
        protected ParsedSqlQuery ParseSqlQueryInfoByTemplateCTE(SqlParserParameter parser)
        {
            var info = new ParsedSqlQuery(parser.QueryParam) { HasPaging = parser.QueryParam.HasPaging };

            var queryMain = parser.QueryTemplate;

            info.StatementFromAndWhere = parser.QueryTemplate.Replace("#SELECT#", string.Empty).Replace("#ORDERBY#", string.Empty); ;

            //extend joins
            var queryExtendJoins = parser.ExtendJoins != null ? string.Join(Environment.NewLine, parser.ExtendJoins) : string.Empty;
            queryMain = queryMain.Replace("#EXTENDJOIN#", queryExtendJoins).Trim();
            info.StatementFromAndWhere = info.StatementFromAndWhere.Replace("#EXTENDJOIN#", queryExtendJoins).Trim();

            //order by
            info.StatementOrderBy = GenerateSqlOrderBy(parser.QueryParam.OrderBy, useDtoName: true);
            queryMain = queryMain.Replace("#ORDERBY#", string.Empty); //use order by at the end

            //where
            if (parser.Conditions != null) parser.Conditions.RemoveAll(string.IsNullOrWhiteSpace);
            var whereCondition = parser.Conditions?.Count > 0 ? $" AND {string.Join($"{Environment.NewLine}AND ", parser.Conditions)}" : string.Empty;

            queryMain = queryMain.Replace("#CONDITION#", whereCondition);
            info.StatementFromAndWhere = info.StatementFromAndWhere.Replace("#CONDITION#", whereCondition).Trim();

            //check: if order-fields not in select-fields -> include order-fields to select-fields
            if (parser.QueryParam.OrderBy?.Count > 0)
            {
                foreach (var item in parser.QueryParam.OrderBy.Where(a => parser.QueryParam.Fields.All(b => a.Field.DtoName != b.DtoName)))
                {
                    if (!parser.TmpTables.Any(x => x.SelectFiels.Any(y => y == item.Field.SqlName)))
                        parser.QueryParam.Fields.Add(item.Field);
                }
            }

            //select
            var selectFields = GenerateSqlSelect(parser.QueryParam.Fields, includeKeywordSelect: false, includeKeyworkDistinct: false);

            //2 ^ 31 https://docs.microsoft.com/en-us/previous-versions/sql/sql-server-2005/ms143179(v=sql.90)?redirectedfrom=MSDN
            var statementSelectNoRowNumber = $"SELECT DISTINCT {selectFields}";

            info.StatementSelect = $"SELECT DISTINCT {selectFields}";

            info.QueryNoRowNumber = queryMain.Replace("#SELECT#", statementSelectNoRowNumber).Trim();

            queryMain = queryMain.Replace("#SELECT#", info.StatementSelect).Trim();


            var filterResultByColumnConditionQuery = parser.FilterResultByColumnConditions?.Count > 0 ? $" WHERE {string.Join($"{Environment.NewLine}AND ", parser.FilterResultByColumnConditions)}" : string.Empty;

            queryMain = $@"WITH #QueryMainQuery as (
                                    {queryMain}
                                ),";


            // join CTEs
            var ctes = parser.OtherCTEs.Select(x => x.CTE).Any() ? string.Join(", ", parser.OtherCTEs.Select(x => x.CTE)) + " , " : " ";
            var joinCTE = parser.OtherCTEs.Select(x => x.Join).Any() ? string.Join("  ", parser.OtherCTEs.Select(x => x.Join)) : " ";
            var selectCTE = parser.OtherCTEs.Select(x => x.Select).Any() ? " , " + string.Join(" , ", parser.OtherCTEs.Select(x => x.Select)) : " ";
            var conditionCTE = parser.OtherCTEs.Select(x => x.Condition).Any() ? string.Join(" ", parser.OtherCTEs.Select(x => x.Condition)) : " ";

            // Join temporary Tables
            var tmpTables = parser.TmpTables.Select(x => x.CreateTableQuery).Any() ? string.Join(", ", parser.TmpTables.Select(x => x.CreateTableQuery)) + " ; " : " ";
            var joinTables = parser.TmpTables.Select(x => x.Join).Any() ? string.Join("  ", parser.TmpTables.Select(x => x.Join)) : " ";
            var selectTables = parser.TmpTables.Select(x => x.Select).Any() ? " , " + string.Join("  ", parser.TmpTables.Select(x => x.Select)) : " ";
            var dropTableTmp = parser.TmpTables.Select(x => x.Drop).Any() ? " ; " + string.Join("  ", parser.TmpTables.Select(x => x.Drop)) : " ";

            var conditionTable = parser.TmpTables.Select(x => x.Condition).Any() ? string.Join(" ", parser.TmpTables.Select(x => x.Condition)) : " ";

            queryMain = $@"{tmpTables} {queryMain} {ctes} #QueryFinalTmp as (
                                   SELECT #QueryMainQuery.* {selectCTE} {selectTables} FROM #QueryMainQuery {joinCTE} {joinTables}
                                    {filterResultByColumnConditionQuery} {conditionCTE} {conditionTable}
                                )";

            if (info.HasPaging)
            {
                var dropTableTmpPaging = dropTableTmp;
                //final query
                info.Query = info.QueryFinally = queryMain;

                var queryPagingTmp = info.Query;
                foreach (var item in parser.TmpTables.Select(x => x.TableName))
                {
                    queryPagingTmp = queryPagingTmp.Replace(item, $"{item}Paging");
                    if (!string.IsNullOrEmpty(dropTableTmpPaging))
                        dropTableTmpPaging = dropTableTmpPaging.Replace(item, $"{item}Paging");
                }

                info.QueryPaging = GenerateSqlQueryPagingCTE(info.Query, parser.QueryParam.Paging, orderBy: info.StatementOrderBy).Trim();

                info.QueryTotalRecords = GenerateSqlQueryTotalRecordsCTE(queryPagingTmp).Trim();
                info.QueryFinally = $"{info.QueryPaging}  {Environment.NewLine}  {dropTableTmp} {Environment.NewLine} ;--Query TotalRecords{Environment.NewLine} {info.QueryTotalRecords}  {Environment.NewLine}  {dropTableTmpPaging} ";
            }
            else
            {
                queryMain = $@"{queryMain} SELECT * FROM #QueryFinalTmp {info.StatementOrderBy} ";
                //final query
                info.Query = info.QueryFinally = queryMain;
            }

            return info;
        }


        /// <summary>
        /// Generate SELECT clause
        /// </summary>
        /// <param name="includeKeywordSelect">true -> include "SELECT "</param>
        /// <param name="defaultIfInvalid">true -> *</param>
        /// <param name="includeKeyworkDistinct">only apply includeKeyword=true</param>
        protected string GenerateSqlSelect(IEnumerable<SqlMapDto> fields, bool includeKeywordSelect = true, bool includeKeyworkDistinct = false, bool defaultIfInvalid = true)
        {
            var items = fields.Select(a => string.IsNullOrWhiteSpace(a.DtoName) || a.SqlName.EndsWith($".{a.DtoName}") ? a.SqlName : $"[{a.DtoName}] = {a.SqlName}").ToList();
            if (items.Count == 0 && defaultIfInvalid)
            {
                items.Add("*");
            }

            if (items.Count == 0)
            {
                return string.Empty;
            }

            var select = items.ToStringList($",{NewLine}");
            if (includeKeyworkDistinct) select = $"DISTINCT {select}";
            if (includeKeywordSelect) select = $"SELECT {select}";

            return select;
        }

        /// <summary>
        /// Generate OrderBy clause
        /// </summary>
        /// <param name="includeKeyword">trye -> include "ORDER BY "</param>
        /// <param name="defaultIfInvalid">true -> SELECT 0</param>
        /// <returns></returns>
        protected string GenerateSqlOrderBy(IEnumerable<OrderByParameter> fields, bool includeKeyword = true, bool defaultIfInvalid = true, bool useDtoName = false)
        {
            var keyword = includeKeyword ? "ORDER BY " : string.Empty;
            var orderByFields = new List<string>();

            if (fields != null)
            {
                foreach (var item in fields.Where(a => a.Field != null && a.Field.CanSort))
                {
                    orderByFields.Add($"{(useDtoName ? item.Field.DtoName : item.Field.SqlName)}{(item.Direction == SortDirectionEnum.Descending ? " DESC" : string.Empty)}");
                }
            }

            return orderByFields.Count > 0
                ? $"{keyword}{string.Join(", ", orderByFields)}"
                : defaultIfInvalid ? $"{keyword}(SELECT 0)" : string.Empty;
        }


        /// <summary>
        /// format: [operation]() OVER ({orderBy}) AS [{alias}]
        /// </summary>
        /// <param name="orderBy">"ORDER BY XXX DESC, YYY" or "XXX DESC, YYY"</param>
        /// <param name="alias"></param>
        /// <param name="operation">ROW_NUMBER, RANK, DENSE_RANK</param>
        private string GenerateFieldRowNumber(string orderBy, string alias = "row_num", string operation = "ROW_NUMBER")
        {
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                orderBy = "ORDER BY (select 0)";
            }
            if (!orderBy.StartsWith("ORDER BY", StringComparison.OrdinalIgnoreCase))
            {
                orderBy = $"ORDER BY {orderBy}";
            }

            /*
            - ROW_NUMBER(): This one generates a new row number for every row, regardless of duplicates within a partition.
            - RANK(): This one generates a new row number for every distinct row, leaving gaps between groups of duplicates within a partition.
            - DENSE_RANK(): This one generates a new row number for every distinct row, leaving no gaps between groups of duplicates within a partition
            */

            return $"{operation}() OVER ({orderBy}) AS [{alias}]";
        }

        private string GenerateSqlQueryPaging(string mainQuery, PagingParameter paging, string aliasFieldRowNumber = "row_num", string aliasPagingTable = "#QueryFinalTmp")
        {
            var offset = GetPagingOffset(paging);

            return $@"SELECT * FROM ({Environment.NewLine}{mainQuery}{Environment.NewLine}) as {aliasPagingTable} WHERE [{aliasFieldRowNumber}] BETWEEN {offset.From} AND {offset.To}";
        }

        private string GenerateSqlQueryPagingCTE(string mainQuery, PagingParameter paging, string orderBy, string aliasPagingTable = "#QueryFinalTmp")
        {
            return $@" {mainQuery} SELECT * FROM {aliasPagingTable} {orderBy} OFFSET  {paging.PageSize} * ({paging.PageIndex} - 1) ROWS  FETCH NEXT {paging.PageSize} ROWS ONLY ";
        }

        private string GenerateSqlQueryTotalRecords(string queryTemplate, string condition, string extendJoins = "", SqlMapDto countDistinceField = null)
        {
            var statement = countDistinceField != null ? $"COUNT(DISTINCT {countDistinceField.SqlName})" : "COUNT(1)";

            return queryTemplate.Replace("#SELECT#", $"SELECT {statement}").Replace("#EXTENDJOIN#", extendJoins).Replace("#CONDITION#", condition).Replace("#ORDERBY#", string.Empty);
        }

        private string GenerateSqlQueryTotalRecordsCTE(string mainQuery, string aliasPagingTable = "#QueryFinalTmp")
        {
            return $" {mainQuery} SELECT COUNT(1) FROM {aliasPagingTable} ";
        }

        protected (int From, int To) GetPagingOffset(PagingParameter paging)
        {
            if (paging == null)
            {
                return (1, 20);
            }

            var from = (paging.PageIndex - 1) * paging.PageSize + 1;
            var to = from + paging.PageSize - 1;

            return (from, to);
        }

        protected void PopulateFilterConditionCommon(BaseFilterConditionRequestDto request, List<string> conditions, DynamicParameters parameters)
        {
            if (conditions == null)
            {
                conditions = new List<string>();
            }
            if (parameters == null)
            {
                parameters = new DynamicParameters();
            }
            if (request?.Fields != null)
            {
                foreach (var item in request?.Fields)
                {
                    var searchField = $"@gridFieldSeach{item.Name}{item.FilterType}";
                    var searchFieldParameter = $"gridFieldSeach{item.Name}{item.FilterType}";
                    var condition = "";
                    //checkd ata type
                    if (item.DataType == DataType.DateTime)
                    {
                        //item.Name = $"CONVERT(VARCHAR(25), {item.Name}, 126)";
                        item.Name = $"CAST({item.Name} AS DATE)";
                    }
                    else if (item.DataType == DataType.Boolean)
                    {
                        item.Name = item.Name == "true" ? "1" : "0";
                    }

                    // check filter type
                    if (item.FilterType == FilterType.Contain)
                    {
                        if (item.DataType == DataType.DateTime)
                        {
                            condition = $"CAST({item.Name} AS DATE) = CAST({searchField} AS DATE)";
                        }
                        else
                        {
                            condition = $" {item.Name} LIKE  N'%' + " + searchField + " +'%' ";
                        }

                    }
                    else if (item.FilterType == FilterType.StartWith)
                    {
                        if (item.DataType == DataType.DateTime)
                        {
                            condition = $"CAST({item.Name} AS DATE) >= CAST({searchField} AS DATE)";
                        }
                        else
                        {
                            condition = $" {item.Name} LIKE  N'%' + " + searchField + " ";
                        }
                    }
                    else
                    {
                        if (item.DataType == DataType.DateTime)
                        {
                            condition = $"CAST({item.Name} AS DATE) <= CAST({searchField} AS DATE)";
                        }
                        else
                        {
                            condition = $" {item.Name} LIKE N'+ " + searchField + "+'%' ";
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(condition))
                    {
                        conditions.Add(condition);
                        parameters.Add(searchFieldParameter, item.Value);
                    }
                }

            }
            // filter conditions alway exit at least one 
            if (conditions.Count == 0)
            {
                conditions.Add(" 1 = 1 ");
            }
        }


        #region Private Classes

        protected class ParsedSqlQuery
        {
            public bool HasPaging { get; internal set; }

            public string StatementSelect { get; internal set; }

            public string StatementFromAndWhere { get; internal set; }

            public string StatementOrderBy { get; internal set; }

            public string Query { get; internal set; }

            public string QueryNoRowNumber { get; internal set; }

            public string QueryPaging { get; internal set; }

            public string QueryTotalRecords { get; internal set; }

            public string QueryFinally { get; internal set; }

            public QueryParameter QueryParameter { get; }

            public ParsedSqlQuery(QueryParameter queryParameter)
            {
                QueryParameter = queryParameter;
            }

            #region Extend

            private string _queryExtendAtTop = string.Empty;

            private string _queryExtendAtBottom = string.Empty;

            public void QueryExtend(string atTop = "", string atBottom = "")
            {
                if (!string.IsNullOrWhiteSpace(atTop))
                    _queryExtendAtTop = $";{_queryExtendAtTop}{Environment.NewLine}{atTop};";

                if (!string.IsNullOrWhiteSpace(atBottom))
                    _queryExtendAtBottom = $";{_queryExtendAtBottom}{Environment.NewLine}{atBottom};";

            }

            public string PopulateQueryExtend(string final)
            {
                if (!string.IsNullOrWhiteSpace(_queryExtendAtTop))
                    final = $"{_queryExtendAtTop};{Environment.NewLine}{final}";

                if (!string.IsNullOrWhiteSpace(_queryExtendAtBottom))
                    final = $"{final};{Environment.NewLine}{_queryExtendAtBottom}";

                return final;
            }

            #endregion
        }

        #endregion
    }
}
