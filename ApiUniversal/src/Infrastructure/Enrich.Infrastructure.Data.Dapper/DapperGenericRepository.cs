using Dapper;
using Dommel;
using Enrich.Common;
using Enrich.Common.Helpers;
using Enrich.Core.Infrastructure.Repository;
using Enrich.Dto;
using Enrich.Dto.Base;
using Enrich.Dto.Requests;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Lookup;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace Enrich.Infrastructure.Data.Dapper
{
	public partial class DapperGenericRepository<T> : DapperRepository, IGenericRepository<T> where T : class
	{
		protected readonly string TableName;
		protected readonly string KeyName;

		public DapperGenericRepository(IConnectionFactory connectionFactory)
			: base(connectionFactory)
		{
			var entityType = typeof(T);
			TableName = Resolvers.Table(entityType, connectionFactory.GetDbConnection());
			KeyName = Resolvers.KeyProperties(entityType)?.FirstOrDefault().Property.Name;
		}

		public virtual T AddGet(T obj)
		{
			using (var connection = _connectionFactory.GetDbConnection())
			{
				object id = connection.Insert(obj);
				return FindById(id);
			}
		}

		public virtual async Task<T> AddGetAsync(T obj)
		{
			using (var connection = _connectionFactory.GetDbConnection())
			{
				object id = await connection.InsertAsync(obj);
				return await FindByIdAsync(id);
			}
		}

		/// <summary>
		/// Inserted T into the database and returns the id.
		/// </summary>
		public virtual async Task<object> AddAsync(T obj)
			=> await base.AddAsync(obj);

		public virtual int Delete(T obj)
		{
			using (var connection = _connectionFactory.GetDbConnection())
			{
				bool isSuccess = connection.Delete(obj);
				if (isSuccess == true)
					return 1;
			}

			return 0;
		}

		public virtual async Task<int> DeleteAsync(T obj)
		{
			using (var connection = _connectionFactory.GetDbConnection())
			{
				bool isSuccess = await connection.DeleteAsync(obj);
				if (isSuccess == true)
					return 1;
			}

			return 0;
		}

		public virtual int DeleteById(object id)
		{
			var obj = FindById(id);
			if (obj == null) return 0;

			using (var connection = _connectionFactory.GetDbConnection())
			{
				bool isSuccess = connection.Delete(obj);
				if (isSuccess == true)
					return 1;
			}
			return 0;
		}

		public virtual async Task<int> DeleteByIdAsync(object id)
		{
			var obj = await FindByIdAsync(id);
			if (obj == null) return 0;

			using (var connection = _connectionFactory.GetDbConnection())
			{
				bool isSuccess = await connection.DeleteAsync(obj);
				if (isSuccess == true)
					return 1;
			}
			return 0;
		}
		public virtual IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
		{
			throw new NotImplementedException();
		}

		public virtual T FindById(object id)
		{
			using (var connection = _connectionFactory.GetDbConnection())
			{
				return connection.Get<T>(id);
			}
		}

		public virtual async Task<T> FindByIdAsync(object id)
		{
			using (var connection = _connectionFactory.GetDbConnection())
			{
				return await connection.GetAsync<T>(id);
			}
		}

		public virtual async Task<bool> IsExistsAsync(object id)
		{
			var entityInfo = GetEntityInfo(typeof(T));
			var query = $"SELECT TOP 1 1 FROM {entityInfo.TableName} WHERE {entityInfo.KeyName}=@KeyValue";

			using (var connection = _connectionFactory.GetDbConnection())
			{
				var scalarValue = await connection.ExecuteScalarAsync<int>(query, new { KeyValue = id });

				return scalarValue > 0;
			}
		}

		public virtual IEnumerable<T> GetAll()
		{
			using (var connection = _connectionFactory.GetDbConnection())
			{
				return connection.GetAll<T>();
			}
		}

		public virtual async Task<IEnumerable<T>> GetAllAsync()
		{
			using (var connection = _connectionFactory.GetDbConnection())
			{
				return await connection.GetAllAsync<T>();
			}
		}

		public virtual IEnumerable<T> GetByRawSql(string sql, params object[] parameters)
		{
			using (var connection = _connectionFactory.GetDbConnection())
			{
				return connection.Query<T>(sql);
			}
		}

		public virtual IEnumerable<T> GetListPaged(int pageNumber, int rowsPerPage, string conditions, string orderby, object parameters = null)
		{
			throw new NotImplementedException();
		}

		public virtual object GetObjectId(T entity)
		{
			return FindById(entity);
		}

		public virtual int GetTotalCount()
		{
			throw new NotImplementedException();
		}

		public void SetObjectId(T entity, object id)
		{
			//throw new NotImplementedException();
		}

		public virtual int Update(T obj)
		{
			using (var connection = _connectionFactory.GetDbConnection())
			{
				bool isSuccess = connection.Update(obj);
				if (isSuccess == true) return 1;
			}
			return 0;
		}

		public virtual async Task<int> UpdateAsync(T obj)
			=> await base.UpdateAsync(obj);

		public virtual async Task<bool> SaveAsync(object id, T obj, bool? isNew = null)
			=> await base.SaveAsync(id, obj, isNew);

		/// <summary>
		/// valueFieldName (ID)
		/// </summary>
		public virtual async Task<IEnumerable<IdNameDto>> GetIdNamesAsync(LookupDataRequest request)
		{
			return await ExecuteGetIdNamesAsync<IdNameDto>(TableName, KeyName, request.FieldNames, request.SearchField, request.SearchText, string.Join(" AND ", request.ExtendConditions), request.SqlOrderby);
		}

		public async Task<IEnumerable<BaseEvent<TValue>>> GetEventAsync<TValue>(BusinessEventRequest request)
		{
			var entityType = typeof(T);
			var dbFields = Resolvers.Properties(entityType);
			var fieldNames = dbFields.Where(f => f.Property.Name != KeyName).Select(f => new SqlMapDto($"{TableName}.{f.Property.Name}", f.Property.Name)).ToList();
			fieldNames.Add(new SqlMapDto("BusinessEvent.MetaData", "MetaData"));
			var keyName = $"{TableName}.{KeyName}";
			var tableNames = $"{TableName} WITH (NOLOCK) JOIN [BusinessEvent] on {TableName}.BusinessEventId = [BusinessEvent].Id";
			return await ExecuteCustomJoinAsync<BaseEvent<TValue>>(tableNames, keyName, fieldNames, "", request.SearchText, string.Join(" AND ", request.ExtendConditions), request.SqlOrderby);
		}

		#region Save Multiple

		public async Task BulkInsertAsync(IEnumerable<T> entities)
			=> await base.BulkInsertAsync(entities);

		protected async Task<int> SaveChildsAsync(string parentFieldName, object parentId, List<T> childs, bool includeRowCount = true, Func<string, DynamicParameters, (string, DynamicParameters)> callBackSqlInfo = null)
			=> await base.SaveChildsAsync(parentFieldName, parentId, childs, includeRowCount: includeRowCount, callBackSqlInfo: callBackSqlInfo);

		public async Task SaveChangesAsync(ChangeEntity<T> changes)
			=> await base.SaveChangesAsync(changes);

		#endregion

		#region Delete

		/// <summary>
		/// fieldModifiedDate <> empty -> set [fieldModifiedDate] = now
		/// </summary>
		protected async Task<int> UpdateIsDeletedAsync(bool deleted, IEnumerable<int> ids, bool updateModifiedDate, string fieldModifiedDate = "ModifiedDate")
		{
			var sql = $"UPDATE {TableName} SET IsDeleted = @IsDeleted";
			if (updateModifiedDate && !string.IsNullOrWhiteSpace(fieldModifiedDate)) sql += $", [{fieldModifiedDate}] = GETDATE()";
			sql += $" WHERE {KeyName} IN @Ids";

			using (var connection = GetDbConnection())
			{
				return await connection.ExecuteAsync(sql, new { IsDeleted = deleted, Ids = ids });
			}
		}

		#endregion

		#region Query

		public virtual async Task<IEnumerable<T>> QueryAsync(QueryRequest request)
		{
			var sql = GetSqlQueryRequest(request);

			if (sql.Conditions.Count > 0)
			{
				sql.Query += $" WHERE {sql.Conditions.ToStringList(" AND ")}";
			}

			return await ExecuteBySharedAsync(async (con, tran) => await con.QueryAsync<T>(sql.Query, sql.Parameters, transaction: tran));
		}

		private (string Query, List<string> Conditions, DynamicParameters Parameters) GetSqlQueryRequest(QueryRequest request)
		{
			var parameters = new DynamicParameters();
			var conditions = new List<string>();

			if (request?.NotInIds?.Length > 0)
			{
				conditions.Add($"{KeyName} NOT IN ({request.NotInIds.ToStringList()})");
			}

			if (request?.InIds?.Length > 0)
			{
				conditions.Add($"{KeyName} IN ({request.InIds.ToStringList()})");
			}

			if (!string.IsNullOrWhiteSpace(request?.StartsWith))
			{
				var fieldNames = request.StartsWithLanguage.GetFieldNames(request.StartsWithPrefixFieldName);
				if (fieldNames.Any())
				{
					conditions.Add($"({string.Join(" OR ", fieldNames.Select(field => $"{field} LIKE @StartsWith"))})");
					parameters.Add("StartsWith", $"{request.StartsWith}%");
				}
			}

			return ($"SELECT * FROM {TableName}", conditions, parameters);
		}

		public async Task<int> UpdateValuesByIdAsync(object id, params (Expression<Func<T, object>> Field, object Value)[] updateFields)
			=> await base.UpdateValuesByIdAsync<T>(id, updateFields);

		public async Task<int> UpdateValuesByIdAsync(object id, Dictionary<string, object> updateFields)
			=> await base.UpdateValuesByIdAsync<T>(id, updateFields);

		public async Task<int> UpdateValuesByConditionsAsync(Dictionary<Expression<Func<T, object>>, object> keyFields, Dictionary<Expression<Func<T, object>>, object> updateFields)
			=> await base.UpdateValuesByConditionsAsync<T>(keyFields, updateFields);

		public async Task<int> UpdateValuesByConditionsAsync(Dictionary<string, object> keyFields, Dictionary<string, object> updateFields)
			=> await base.UpdateValuesByConditionsAsync<T>(keyFields, updateFields);

		public async Task<int> UpdatePropValuesByKeysAsync(Dictionary<string, object> keyFields, Dictionary<string, object> updateFields)
			=> await base.UpdateValuesByConditionsAsync<T>(keyFields, updateFields);

		#endregion
	}
}
