using Dapper;
using Promotion.Model.Model.Comon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Extensions.Helper
{
    public static class ContextHelper
    {
        public static IDbConnection AutoConnect(this IDbConnection cnn)
        {
            if (cnn.State == ConnectionState.Closed)
            {
                cnn = new SqlConnection(Const.POS_CONNECTION_STRING);
                cnn.Open();
            }
            return cnn;
        }
        public static IDbConnection ImsAutoConnect(this IDbConnection cnn)
        {
            if (cnn.State == ConnectionState.Closed)
            {
                cnn = new SqlConnection(Const.IMS_CONNECTION_STRING);
                cnn.Open();
            }
            return cnn;
        }
        public static IDbConnection RCPAutoConnect(this IDbConnection cnn)
        {
            if (cnn.State == ConnectionState.Closed)
            {
                cnn = new SqlConnection(Const.RCP_CONNECTION_STRING);
                cnn.Open();
            }
            return cnn;
        }
        public static T SqlFirstOrDefault<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            T result;
            try
            {
                result = cnn.QueryFirstOrDefault<T>(sql, param, transaction, commandTimeout, commandType);
            }
            catch
            {
                cnn.Close();
                throw;
            }
            return result;
        }

        public static Task<T> SqlFirstOrDefaultAsync<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            Task<T> result;
            try
            {
                result = cnn.QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandTimeout, commandType);
            }
            catch
            {
                cnn.Close();
                throw;
            }
            return result;
        }

        public static IEnumerable<T> SqlQuery<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            IEnumerable<T> result;
            try
            {
                result = cnn.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);
            }
            catch
            {
                cnn.Close();
                throw;
            }
            return result;
        }
        
        public static Task<IEnumerable<T>> SqlQueryAsync<T>(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            Task<IEnumerable<T>> result;
            try
            {
                result = cnn.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType);
            }
            catch
            {
                cnn.Close();
                throw;
            }
            return result;
        }

    }
}
