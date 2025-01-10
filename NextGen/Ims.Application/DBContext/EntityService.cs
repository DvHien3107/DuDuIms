using Dapper;
using Promotion.Application.Interfaces;
using Promotion.Model.Model.Comon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Promotion.Application.Extensions
{
    public class EntityService<T> : IDisposable, IEntityService<T> where T : class
    {
        public IDbConnection _connection;

        public void Dispose()
        {
            _connection.Dispose();
            _connection.Close();
        }

        public IEnumerable<T> Query(string sql, object param = null)
        {

            if (_connection.State == ConnectionState.Closed)
                _connection.Open();

            return _connection.Query<T>(sql, param).AsEnumerable();
        }

        public async Task<IEnumerable<T>> QueryAsync(string sql, object param = null)
        {

            if (_connection.State == ConnectionState.Closed)
                _connection.Open();

            return await _connection.QueryAsync<T>(sql, param);
        }

        public void Execute(string sql, object param = null)
        {

            if (_connection.State == ConnectionState.Closed)
                _connection.Open();

            _connection.Execute(sql, param);
        }
        public async Task ExecuteAsync(string sql, object param = null)
        {

            if (_connection.State == ConnectionState.Closed)
                _connection.Open();

            await _connection.ExecuteAsync(sql, param);
        }


        public async Task<T> Get(object id)
        {
            string sql = $@"select * from {ObjectHelper.TableName<T>()}  WITH(NOLOCK) where {ObjectHelper.PrimaryKey<T>()}=@id";
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();

            return (await _connection.QueryAsync<T>(sql, new { id = id })).FirstOrDefault();
        }

        public async Task<T> Find(string condition, object parms = null)
        {
            string sql = $@"select * from {ObjectHelper.TableName<T>()}  WITH(NOLOCK)";
            if (!string.IsNullOrWhiteSpace(condition))
            {
                sql += $@" where {condition}";
            }
            var result = (await _connection.QueryAsync<T>(sql, parms)).FirstOrDefault();
            return result;
        }

        public async Task<IEnumerable<T>> GetAll(string condition = null, object parms = null, string orderBy = "", int? page = null, int? limit = Const.Limit)
        {
            string sql = $@"select * from {ObjectHelper.TableName<T>()} WITH(NOLOCK)";
            if (!string.IsNullOrWhiteSpace(condition))
            {
                sql += $@" where {condition}";
            }
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                sql += $@" Order By {orderBy}";
            }

            if (page >= 0)
            {
                sql += $@" OFFSET {page * limit} ROWS
                        FETCH NEXT  {limit} ROWS ONLY";
            }
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
            return await _connection.QueryAsync<T>(sql, parms);
        }
        public async Task<IEnumerable<T>> GetAll(string select, string from, string condition = null,
           object parms = null, string orderBy = "", int? page = null, int? limit = 50)
        {
            if (string.IsNullOrWhiteSpace(from))
            {
                from = ObjectHelper.TableName<T>() + " with(nolock)";
            }
            string sql = $@"select {select} from {from} ";

            if (!string.IsNullOrWhiteSpace(condition))
            {
                sql += $@" where {condition}";
            }
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                sql += $@" Order By {orderBy}";
            }
            if (page >= 0)
            {
                sql += $@" OFFSET {page * limit} ROWS
                        FETCH NEXT  {limit} ROWS ONLY";
            }

            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
            return await _connection.QueryAsync<T>(sql, parms);
        }

        public async Task<int> Count(string from, string condition = null, object parms = null)
        {
            from = string.IsNullOrWhiteSpace(from) ? ObjectHelper.TableName<T>() : from;
            string sql = $@"select count(1) from {from} WITH(NOLOCK)";
            if (!string.IsNullOrWhiteSpace(condition))
            {
                sql += $@" where {condition}";
            }

            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
            return (await _connection.QueryAsync<int>(sql, parms)).FirstOrDefault();
        }

        public long Inserts(T parms, IDbTransaction transaction = null)
        {
            long result;

            try
            {
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();

                if (transaction != null)
                {
                    try
                    {
                        result = _connection.Insert<long, T>(parms, transaction);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
                else
                {
                    try
                    {
                        result = _connection.Insert<long, T>(parms);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return result;
        }

        public bool InsertMany(List<T> parms, IDbTransaction transaction = null)
        {
            bool result = false;
            try
            {
                DataTable dt = parms.ToDataTable();
                using (SqlBulkCopy sqlBulk = new SqlBulkCopy((SqlConnection)_connection))
                {
                    if (_connection.State == ConnectionState.Closed)
                        _connection.Open();

                    sqlBulk.DestinationTableName = ObjectHelper.TableName<T>();
                    foreach (DataColumn column in dt.Columns)
                    {
                        sqlBulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }
                    sqlBulk.WriteToServer(dt);
                }
                //  var rows = _connection.BulkInsert(parms);
                result = true;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();
            }

            return result;
        }

        public bool Updates(T parms, IDbTransaction transaction = null)
        {
            int result;
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();
                if (transaction != null)
                {
                    try
                    {
                        result = _connection.Update<T>(parms, transaction);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
                else
                {
                    try
                    {
                        result = _connection.Update(parms);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result > 0 ? true : false;
        }
        //public void UpdateMany(List<T> parms, IDbTransaction transaction = null)
        //{
        //    bool result = false;
        //    try
        //    {
        //        DataTable dt = parms.ToDataTable();
        //        using (SqlConnection con = new SqlConnection(Constants.String.Connectionstring))
        //        {
        //            _connection.CreateCommand
        //            using (SqlCommand command = new SqlCommand("", _connection))
        //            {
        //                try
        //                {
        //                    con.Open();
        //                    //Creating temp table on database
        //                    command.CommandText = "CREATE TABLE #TmpTable(...)";
        //                    command.ExecuteNonQuery();

        //                    using (SqlBulkCopy sqlBulk = new SqlBulkCopy(con))
        //                    {
        //                        //con.Open();
        //                        sqlBulk.DestinationTableName = "#TmpTable";
        //                        foreach (DataColumn column in dt.Columns)
        //                        {
        //                            sqlBulk.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        //                        }
        //                        sqlBulk.WriteToServer(dt);
        //                    }

        //                    string sql = $@"{ObjectHelper.UpdateFromTable<T>()} FROM {ObjectHelper.TableName<T>()} AS P INNER JOIN #TmpTable AS T ON P.[ID] = T.[ID] ;DROP TABLE #TmpTable;";

        //                    command.CommandText = sql;
        //                    command.ExecuteNonQuery();

        //                }
        //                catch (Exception ex) { }
        //            }
        //        }



        //        //var rows = _connection.BulkUpdate(parms);
        //        result = true;

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        if (_connection.State == ConnectionState.Open)
        //            _connection.Close();
        //    }
        //}

        public bool Delete(object id)
        {
            var result = false;
            try
            {
                string sql = $@"delete {ObjectHelper.TableName<T>()} where {ObjectHelper.PrimaryKey<T>()} = @id";
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();

                var rows = _connection.Execute(sql, new { id = id });
                result = rows > 0 ? true : false;
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public async Task<bool> DeleteAsync(object id)
        {
            var result = false;
            try
            {
                string sql = $@"delete {ObjectHelper.TableName<T>()} where {ObjectHelper.PrimaryKey<T>()} = @id";
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();

                var rows = await _connection.ExecuteAsync(sql, new { id = id });
                result = rows > 0 ? true : false;
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public bool Deactivate(object id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeactivateAsync(object id)
        {
            throw new NotImplementedException();
        }


        public IEnumerable<T> Query(string sql, object parms, IDbTransaction transaction = null)
        {
            IEnumerable<T> result;

            try
            {
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();
                if (transaction != null)
                {
                    try
                    {
                        result = _connection.Query<T>(sql, parms, transaction).AsEnumerable();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
                else
                {
                    try
                    {
                        result = _connection.Query<T>(sql, parms).AsEnumerable();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        public IEnumerable<T> Query(string sql, object parms, int? page = null, int? limit = 50)
        {
            IEnumerable<T> result;

            try
            {
                if (page >= 0)
                {
                    sql += $@" OFFSET {page * limit} ROWS
                        FETCH NEXT  {limit} ROWS ONLY";
                }

                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();

                try
                {
                    result = _connection.Query<T>(sql, parms).AsEnumerable();
                }
                catch (Exception ex)
                {
                    throw ex;
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
    }
}
