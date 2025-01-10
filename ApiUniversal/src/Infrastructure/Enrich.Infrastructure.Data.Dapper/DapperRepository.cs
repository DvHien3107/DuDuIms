using Dapper;
using Dapper.FluentMap;
using Dapper.FluentMap.Configuration;
using Dapper.FluentMap.Dommel.Mapping;
using Dommel;
using Enrich.Common;
using Enrich.Common.Helpers;
using Enrich.Core.Infrastructure.Repository;
using Enrich.Dto;
using Enrich.Dto.Base;
using Enrich.Infrastructure.Data.ConectionFactory;
using Enrich.Infrastructure.Data.Dapper.Library;
using FastMember;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.Data.Dapper
{
    public partial class DapperRepository : IRepository
    {
        protected readonly IConnectionFactory _connectionFactory;

        protected string NewLine => System.Environment.NewLine;

        public DapperRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        protected IDbConnection GetDbConnection()
        {
            return _connectionFactory.GetDbConnection();
        }

        protected string GetConnectionString()
        {
            return _connectionFactory.ConnectionString;
        }

        protected async Task<IEnumerable<TDto>> ExecuteGetIdNamesAsync<TDto>(string tableName, string fieldId, List<SqlMapDto> fieldNames, string searchField = "", string searchText = "", string extendConditions = "", string extendOrderBy = "")
        {
            object param = null;
            var query = $"SELECT {fieldId} as Id, {string.Join(", ", fieldNames.Select(a => $"{a.SqlName} as {a.DtoName}"))} from {tableName} where 1=1";

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                if (string.IsNullOrEmpty(searchField))
                {
                    searchField = fieldNames[0].SqlName;
                }

                param = new { SeachText = $@"%{searchText}%" };
                query += $" AND {searchField} like @SeachText";
            }

            if (!string.IsNullOrWhiteSpace(extendConditions))
            {
                query += $" AND {extendConditions}";
            }

            if (!string.IsNullOrWhiteSpace(extendOrderBy))
            {
                query += $" ORDER BY {extendOrderBy}";
            }

            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<TDto>(query, param);
            }
        }

        protected async Task<IEnumerable<TDto>> ExecuteCustomJoinAsync<TDto>(
            string tableName, string fieldId, List<SqlMapDto> fieldNames, 
            string searchField = "", string searchText = "", string extendConditions = "", string extendOrderBy = "")
        {
            object param = null;
            var query = @$"SELECT {fieldId} as Id, {string.Join(", ", fieldNames.Select(a => $"{a.SqlName} as {a.DtoName}"))}
                            FROM {tableName} where 1=1";

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                if (string.IsNullOrEmpty(searchField))
                {
                    searchField = fieldNames[0].SqlName;
                }

                param = new { SeachText = $@"%{searchText}%" };
                query += $" AND {searchField} like @SeachText";
            }

            if (!string.IsNullOrWhiteSpace(extendConditions))
            {
                query += $" AND {extendConditions}";
            }

            if (!string.IsNullOrWhiteSpace(extendOrderBy))
            {
                query += $" ORDER BY {extendOrderBy}";
            }

            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<TDto>(query, param);
            }
        }

        #region Save multiple

        public async Task SaveChangesAsync<TEntity>(ChangeEntity<TEntity> changes)
        {
            //await SaveChangesByMergeAsync(changes);
            await SaveChangesByQueryAsync(changes);
        }

        public async Task SaveChangesAsync(bool forEach, params (string Query, List<(string Key, object Value)> Parameters)[] changes)
        {
            await ExecuteBySharedAsync(async (con, tran) =>
            {
                if (forEach)
                {
                    foreach (var change in changes)
                    {
                        var parameters = new DynamicParameters();
                        change.Parameters.ForEach(a => parameters.Add(a.Key, a.Value));

                        await con.ExecuteAsync(change.Query, parameters, transaction: tran);
                    }
                }
                else
                {
                    var queries = new List<string>();
                    var parameters = new DynamicParameters();
                    foreach (var change in changes)
                    {
                        queries.Add(change.Query);
                        change.Parameters.ForEach(a => parameters.Add(a.Key, a.Value));
                    }

                    await con.ExecuteAsync(queries.ToStringList(NewLine), parameters, transaction: tran);
                }

                return true;
            });
        }

        private async Task SaveChangesByMergeAsync<TEntity>(ChangeEntity<TEntity> changes)
        {
            if (!changes.HasChanges)
            {
                return;
            }

            await ExecuteBySharedAsync(async (con, tran) =>
            {
                //insert & update
                if (changes.HasAdded || changes.HasModified)
                {
                    var insertUpdates = new List<TEntity>();
                    if (changes.HasAdded) insertUpdates.AddRange(changes.Added);
                    if (changes.HasModified) insertUpdates.AddRange(changes.Modified);

                    var info = GenerateSqlMerge(insertUpdates, includeRowCount: false);
                    await con.ExecuteAsync(info.Query, info.Parameters, transaction: tran);
                }

                //delete
                if (changes.HasDeleted)
                {
                    var entityInfo = GetEntityInfo(typeof(TEntity));
                    var queryDeleted = $"DELETE {entityInfo.TableName} WHERE {entityInfo.KeyName} IN @DeletedIds";

                    await con.ExecuteAsync(queryDeleted, new { DeletedIds = changes.Deleted }, transaction: tran);
                }

                return true;
            });
        }

        private async Task SaveChangesByQueryAsync<TEntity>(ChangeEntity<TEntity> changes)
        {
            var info = GenerateSqlSaveChanges(changes);

            if (string.IsNullOrWhiteSpace(info.Query))
            {
                return;
            }

            await ExecuteBySharedAsync(async (con, tran) => await con.ExecuteAsync(info.Query, info.Parameters, transaction: tran));
        }

        protected async Task<int> SaveChildsAsync<TEntity>(string parentFieldName, object parentId, List<TEntity> childs, bool includeRowCount = true, Func<string, DynamicParameters, (string, DynamicParameters)> callBackSqlInfo = null)
        {
            if (childs == null)
            {
                return -1;
            }

            using (var connection = GetDbConnection())
            {
                var info = GenerateSqlMerge(childs, parentFieldName, parentId, includeRowCount: includeRowCount);
                if (callBackSqlInfo != null)
                {
                    info = callBackSqlInfo(info.Query, info.Parameters);
                }

                if (string.IsNullOrWhiteSpace(info.Query))
                {
                    return -1;
                }

                return await connection.ExecuteAsync(info.Query, info.Parameters);
            }
        }

        //parentFieldName: use for DELETE statement, if empty -> only INSERT, UPDATE
        protected (string Query, DynamicParameters Parameters) GenerateSqlMerge<TEntity>(List<TEntity> entities, string parentFieldName = "", object parentFieldValue = null, bool includeRowCount = true)
        {
            var typeEntity = typeof(TEntity);
            var entityInfo = GetEntityInfo(typeEntity);
            var includeDeleteStatement = !string.IsNullOrWhiteSpace(parentFieldName) && parentFieldValue != null;

            var parameters = new DynamicParameters();
            if (includeDeleteStatement) parameters.Add("@ParentFieldValue", parentFieldValue);

            var entityProps = typeEntity.GetProperties();
            var allColNamesWithoutKey = entityProps.Where(p => p.Name != entityInfo.KeyName).Select(p => p.Name).ToList();

            var joinValues = new List<string>();

            if (entities.Count > 0) //have data
            {
                for (int i = 0; i < entities.Count; i++)
                {
                    var colValues = new List<string>();

                    foreach (var prop in entityProps)
                    {
                        colValues.Add($"@{prop.Name}{i} as {prop.Name}");
                        parameters.Add($"{prop.Name}{i}", prop.GetValue(entities[i]));
                    }

                    joinValues.Add($@"SELECT {string.Join(", ", colValues)}");
                }
            }
            else//empty data
            {
                var colValues = entityProps.Select(a => $@"NULL as {a.Name}");
                joinValues.Add($"SELECT {string.Join(", ", colValues)} WHERE 1 = 0"); //to return empty data
            }

            var tableValues = string.Join($"{Environment.NewLine} UNION {Environment.NewLine}", joinValues);
            var sourceCols = string.Join(", ", entityProps.Select(p => p.Name));

            var updateCols = allColNamesWithoutKey.Select(name => $"TARGET.{name} = SOURCE.{name}");
            var insertCols = string.Join(", ", allColNamesWithoutKey);
            var insertValues = string.Join(", ", allColNamesWithoutKey.Select(col => $"SOURCE.{col}"));

            var sql = $@"MERGE {entityInfo.TableName} as TARGET
USING (
    {tableValues}
) AS SOURCE ({sourceCols})
ON TARGET.{entityInfo.KeyName} = SOURCE.{entityInfo.KeyName}

WHEN MATCHED THEN --UPDATE
    UPDATE SET {string.Join(", ", updateCols)}

WHEN NOT MATCHED BY TARGET THEN --INSERT
    INSERT({insertCols}) VALUES({insertValues})
";

            if (includeDeleteStatement)
            {
                sql += $@"{NewLine}WHEN NOT matched BY SOURCE AND TARGET.{parentFieldName} = @ParentFieldValue --DELETE
    THEN DELETE";
            }

            sql += $";{NewLine}{(includeRowCount ? "SELECT @@ROWCOUNT;" : string.Empty)}";

            return (sql, parameters);
        }

        public (string Query, DynamicParameters Parameters) GenerateSqlSaveChanges<TEntity>(ChangeEntity<TEntity> changes)
        {
            var infos = GenerateSqlChanges(changes);

            var parameters = new DynamicParameters();
            foreach (var item in infos.Parameters)
            {
                parameters.Add(item.Key, item.Value);
            }

            return (infos.Query, parameters);
        }

        public (string Query, List<(string Key, object Value)> Parameters) GenerateSqlChanges<TEntity>(ChangeEntity<TEntity> changes)
        {
            if (!changes.HasChanges)
            {
                return (string.Empty, new List<(string Key, object Value)>());
            }

            var queries = new List<string>();
            var parameters = new List<(string Key, object Value)>();

            var entityType = typeof(TEntity);
            var entityProps = entityType.GetProperties();
            var entityInfo = GetEntityInfo(entityType);

            List<PropertyInfo> getEntityProps(List<string> ignoreFields = null)
            {
                var props = entityProps.Where(p => ignoreFields == null || !ignoreFields.Contains(p.Name)).ToList();
                if (entityInfo.KeyIsIdentity)
                {
                    props.RemoveAll(a => a.Name == entityInfo.KeyName);
                }

                return props;
            }

            //added
            if (changes.Added?.Count > 0)
            {
                queries.Add($"--{entityInfo.TableName}: Insert {changes.Added.Count}");
                var insertProps = getEntityProps(changes.IgnoreInsertFields);

                //format: insert ... values ()
                foreach (var item in changes.Added)
                {
                    var colNames = new List<string>();
                    var colValues = new List<string>();

                    foreach (var prop in insertProps)
                    {
                        var propValue = prop.GetValue(item);
                        if (propValue == null)
                        {
                            continue;
                        }

                        colNames.Add($"[{prop.Name}]");

                        var data = GetSqlParamData(propValue);
                        if (data.AsParamName)
                        {
                            colValues.Add($"@{data.Value}");
                            parameters.Add((data.Value, propValue));
                        }
                        else
                        {
                            colValues.Add(data.Value);
                        }
                    }

                    queries.Add($"INSERT INTO {entityInfo.TableName} ({colNames.ToStringList()}){NewLine}VALUES ({colValues.ToStringList()})");
                }
            }

            //modified
            if (changes.Modified?.Count > 0)
            {
                queries.Add($"--{entityInfo.TableName}: Update {changes.Modified.Count}");
                var updateProps = getEntityProps(changes.IgnoreUpdateFields);

                foreach (var item in changes.Modified)
                {
                    //set
                    var updateSets = new List<string>();
                    foreach (var prop in updateProps)
                    {
                        var propValue = prop.GetValue(item);
                        var data = GetSqlParamData(propValue);

                        if (data.AsParamName)
                        {
                            updateSets.Add($"[{prop.Name}]=@{data.Value}");
                            parameters.Add(($"{data.Value}", propValue));
                        }
                        else
                        {
                            updateSets.Add($"[{prop.Name}]={data.Value}");
                        }
                    }

                    //where
                    var condition = string.Empty;
                    var keyValue = entityProps.First(a => a.Name == entityInfo.KeyName).GetValue(item);

                    var keyData = GetSqlParamData(keyValue);
                    if (keyData.AsParamName)
                    {
                        condition = $"{entityInfo.KeyName}=@{keyData.Value}";
                        parameters.Add((keyData.Value, keyValue));
                    }
                    else
                    {
                        condition = $"{entityInfo.KeyName}={keyData.Value}";
                    }

                    //final
                    queries.Add($"UPDATE {entityInfo.TableName} SET {updateSets.ToStringList(", ")} WHERE {condition}");
                }
            }

            //deleted
            if (changes.Deleted?.Count > 0)
            {
                queries.Add($"--{entityInfo.TableName}: Delete {changes.Deleted.Count}");
                queries.Add(changes.UpdateIsDeleted
                    ? $"UPDATE {entityInfo.TableName} SET IsDeleted=1 WHERE {entityInfo.KeyName} IN ({changes.Deleted.ToStringList()})"
                    : $"DELETE {entityInfo.TableName} WHERE {entityInfo.KeyName} IN ({changes.Deleted.ToStringList()})");
            }

            return (string.Join(NewLine, queries), parameters);
        }

        private (string Value, bool AsParamName) GetSqlParamData(object value)
        {
            if (value == null)
                return ("NULL", false);

            if (value is int || value is short || value is long || value is ushort || value is uint || value is ulong)
                return ($"{value}", false);

            if (value is double valDouble)
                return (valDouble.SqlVal(), false);

            if (value is float valFloat)
                return (valFloat.SqlVal(), false);

            if (value is decimal valDecimal)
                return (valDecimal.SqlVal(), false);

            if (value is bool valBool)
                return ($"{(valBool ? 1 : 0)}", false);

            return (Guid.NewGuid().ToString("N"), true);
        }

        protected async Task BulkInsertAsync<TEntity>(IEnumerable<TEntity> entities)
        {
            if (entities == null || entities.Count() == 0)
                return;

            var entityType = typeof(TEntity);
            using (var sqlCopy = new SqlBulkCopy(GetConnectionString()))
            {
                sqlCopy.DestinationTableName = Resolvers.Table(entityType, GetDbConnection());
                sqlCopy.BatchSize = 500;

                var tableMappings = FluentMapper.EntityMaps.FirstOrDefault(c => c.Key.Name == entityType.Name);
                var dbFields = Resolvers.Properties(entityType).ToList();
                dbFields.ForEach(field =>
                {
                    var keyMap = tableMappings.Value.PropertyMaps.FirstOrDefault(m => m.PropertyInfo.Name == field.Property.Name);
                    if (keyMap != null)
                    {
                        sqlCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(keyMap.PropertyInfo.Name, keyMap.ColumnName));
                    }
                    else
                    {
                        sqlCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(field.Property.Name, field.Property.Name));
                    }
                });

                using (var reader = ObjectReader.Create(entities, Resolvers.Properties(entityType).Select(p => p.Property.Name).ToArray()))
                {
                    await sqlCopy.WriteToServerAsync(reader);
                }
            }
        }

        #endregion

        /// <summary>
        /// Inserted TEntity into the database and returns the id.
        /// </summary>
        public async Task<object> AddAsync<TEntity>(TEntity obj) where TEntity : class
        {
            return await ExecuteBySharedAsync(async (con, tran) => await con.InsertAsync(obj, tran));
        }

        public async Task<int> UpdateAsync<TEntity>(TEntity obj) where TEntity : class
        {
            return await ExecuteBySharedAsync(async (con, tran) =>
            {
                var success = await con.UpdateAsync(obj, tran);
                return success ? 1 : 0;
            });
        }

        public async Task<bool> SaveAsync<TEntity>(object id, TEntity obj, bool? isNew = null) where TEntity : class
        {
            return await ExecuteBySharedAsync(async (con, tran) =>
            {
                if (isNew.GetValueOrDefault())
                {
                    return await con.InsertAsync(obj, tran) != null;
                }

                //save: insert or update
                var info = GetEntityInfo(typeof(TEntity));

                var queryTop1 = $"SELECT TOP (1) 1 FROM {info.TableName} WHERE {info.KeyName}=@KeyValue";
                var top1 = await con.QueryFirstOrDefaultAsync(queryTop1, new { KeyValue = id }, transaction: tran);

                return top1 != null
                    ? await con.UpdateAsync(obj, tran)
                    : await con.InsertAsync(obj, tran) != null;
            });
        }

        protected (string TableName, string KeyName, bool KeyIsIdentity) GetEntityInfo(Type typeEntity)
        {
            var keyColumn = Resolvers.KeyProperties(typeEntity).FirstOrDefault();

            return (Resolvers.Table(typeEntity, GetDbConnection()), keyColumn.Property.Name, true);
        }

        protected string CreateParamOut(DynamicParameters parameters, DbType dbType, int size, bool prefix = true)
        {
            var param = CreateParamName(false);
            parameters.Add(param, direction: ParameterDirection.Output, size: size);

            return prefix ? $"@{param}" : param;
        }

        protected string CreateParam(DynamicParameters parameters, object value, bool prefix = true)
        {
            var param = CreateParamName(false);
            parameters.Add(param, value);

            return prefix ? $"@{param}" : param;
        }

        protected string CreateParamName(bool prefix = true)
        {
            var param = Guid.NewGuid().ToString().Replace("-", string.Empty);
            return prefix ? $"@{param}" : param;
        }

        protected void InitParameters<TEntity>(DynamicParameters parameters, TEntity source) where TEntity : class
        {
            foreach (var item in Resolvers.Properties(typeof(TEntity)))
            {
                parameters.Add(item.Property.Name, item.Property.GetValue(source));
            }
        }

        /// <summary>
        /// safeName = true -> xxx -> [xxx]
        /// </summary>
        protected List<string> GetFieldNames<TEntity>(bool safeName = false) where TEntity : class
        {
            return GetFields<TEntity>().Select(a => safeName ? $"[{a.Name}]" : a.Name).ToList();
        }

        protected IEnumerable<PropertyInfo> GetFields<TEntity>() where TEntity : class => Resolvers.Properties(typeof(TEntity)).Select(x => x.Property);

        public virtual Task<object> GetValueByIdAsync(object id, string fieldName) => null;

        public async Task<IEnumerable<TEntity>> GetValuesByConditionsAsync<TEntity>(List<(string Key, object Value)> conditions, bool conditionOr = false, params Expression<Func<TEntity, object>>[] selectFields)
        {
            var info = GetEntityInfo(typeof(TEntity));

            //select
            var fieldNames = selectFields.Select(a => a.MemberName()).Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
            if (fieldNames.Count == 0) fieldNames.Add("*");

            //condition
            var parameters = new DynamicParameters();
            var sqlConditions = new List<string>();
            if (conditions?.Count > 0)
            {
                foreach (var field in conditions)
                {
                    sqlConditions.Add($"{field.Key} = @{field.Key}");
                    parameters.Add(field.Key, field.Value);
                }
            }

            var sqlQuery = $"SELECT {fieldNames.ToStringList()} FROM {info.TableName}";
            if (sqlConditions.Count > 0)
                sqlQuery += $" WHERE {sqlConditions.ToStringList(conditionOr ? " OR " : " AND ")}";

            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<TEntity>(sqlQuery, parameters);
            }
        }

        public async Task<int> UpdateValuesByIdAsync<TEntity>(object id, params (Expression<Func<TEntity, object>> Field, object Value)[] updateFields)
        {
            if (updateFields.IsNullOrEmpty())
            {
                return -1;
            }

            var finalUpdateFields = updateFields.Select(a => new { Field = a.Field.MemberName(), a.Value })
                .Where(a => !string.IsNullOrWhiteSpace(a.Field))
                .GroupBy(a => a.Field)
                .ToDictionary(k => k.Key, v => v.First().Value);

            if (finalUpdateFields.Count == 0)
            {
                return -1;
            }

            return await UpdateValuesByIdAsync<TEntity>(id, finalUpdateFields);
        }

        public async Task<int> UpdateValuesByIdAsync<TEntity>(object id, Dictionary<string, object> updateFields)
        {
            var info = GetEntityInfo(typeof(TEntity));
            var keyFields = new Dictionary<string, object> { [info.KeyName] = id };

            return await UpdateValuesByConditionsAsync<TEntity>(keyFields, updateFields);
        }

        public async Task<int> UpdateValuesByConditionsAsync<TEntity>(Dictionary<Expression<Func<TEntity, object>>, object> keyFields, Dictionary<Expression<Func<TEntity, object>>, object> updateFields)
        {
            var dicKeyFields = new Dictionary<string, object>();
            foreach (var item in keyFields)
            {
                var memberInfo = item.Key.Body.MemberInfo();
                if (memberInfo != null)
                {
                    dicKeyFields.Add(memberInfo.Name, item.Value);
                }
            }

            var dicUpdateFields = new Dictionary<string, object>();
            foreach (var item in updateFields)
            {
                if (item.Key.Body is MemberExpression member)
                {
                    dicUpdateFields.Add(member.Member.Name, item.Value);
                }
            }

            return await UpdateValuesByConditionsAsync<TEntity>(dicKeyFields, dicUpdateFields);
        }

        public async Task<int> UpdateValuesByConditionsAsync<TEntity>(Dictionary<string, object> keyFields, Dictionary<string, object> updateFields)
        {
            var info = GetEntityInfo(typeof(TEntity));

            var param = new DynamicParameters();

            //keys
            foreach (var key in keyFields)
            {
                param.Add($"k_{key.Key}", key.Value);
            }

            //sets
            foreach (var set in updateFields)
            {
                param.Add($"s_{set.Key}", set.Value);
            }

            using (var connection = GetDbConnection())
            {
                var query = $"UPDATE {info.TableName} SET {updateFields.Select(a => $"{a.Key}=@s_{a.Key}").ToStringList()} WHERE {keyFields.Select(a => $"{a.Key}=@k_{a.Key}").ToStringList(" AND ")}";
                return await connection.ExecuteAsync(query, param);
            }
        }

        protected async Task<TResult> ExecuteBySharedAsync<TResult>(Func<IDbConnection, IDbTransaction, Task<TResult>> execute)
        {
            IDbConnection connection = null;

            try
            {
                connection = _connectionFactory.IsSharedConnection ? _connectionFactory.SharedConnection : GetDbConnection();

                return await execute(connection, _connectionFactory.IsSharedConnection ? _connectionFactory.SharedTransaction : null);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!_connectionFactory.IsSharedConnection && connection != null)
                {
                    connection.Dispose();
                }
            }
        }

        /// <summary>
        /// use for insert or update data
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="execute"></param>
        /// <returns></returns>
        protected TResult ExecuteByShared<TResult>(Func<IDbConnection, IDbTransaction, TResult> execute)
        {
            IDbConnection connection = null;

            try
            {
                connection = _connectionFactory.IsSharedConnection ? _connectionFactory.SharedConnection : GetDbConnection();

                return execute(connection, _connectionFactory.IsSharedConnection ? _connectionFactory.SharedTransaction : null);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!_connectionFactory.IsSharedConnection && connection != null)
                {
                    connection.Dispose();
                }
            }
        }

        public async Task<IEnumerable<T>> GetEnumerableAsync<T>(string query, object param = null)
        {
            using (var db = GetDbConnection())
            {
                return await db.QueryAsync<T>(query, param);
            }
        }

        public async Task<T> GetScalarValueAsync<T>(string query, object param = null)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return default(T);
            }

            using (var db = GetDbConnection())
            {
                return await db.ExecuteScalarAsync<T>(query, param);
            }
        }

        public async Task<T> FirstOrDefaultAsync<T>(string query, object param = null)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return default(T);
            }

            using (var db = GetDbConnection())
            {
                return await db.QueryFirstOrDefaultAsync<T>(query, param);
            }
        }

        public int Execute(string sql, object param = null)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                return -1;
            }

            using (var connection = GetDbConnection())
            {
                return connection.Execute(sql, param);
            }
        }

        public async Task<int> ExecuteAsync(string sql, object param = null, bool useShared = false)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                return -1;
            }

            if (useShared)
            {
                return await ExecuteBySharedAsync((con, tran) => con.ExecuteAsync(sql, param, transaction: tran));
            }
            else
            {
                using (var connection = GetDbConnection())
                {
                    return await connection.ExecuteAsync(sql, param);
                }
            }
        }

        public async Task<int> ExecuteStoreAsync(string name, object param = null, bool useShared = false)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return -1;
            }

            if (useShared)
            {
                return await ExecuteBySharedAsync((con, tran) => con.ExecuteAsync(name, param, transaction: tran, commandType: CommandType.StoredProcedure));
            }
            else
            {
                using (var connection = GetDbConnection())
                {
                    return await connection.ExecuteAsync(name, param, commandType: CommandType.StoredProcedure);
                }
            }
        }

        public async Task<IEnumerable<T>> ExecuteStoreAsync<T>(string name, object param = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }
            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<T>(name, param, commandType: CommandType.StoredProcedure);
            }

        }
    }
}
