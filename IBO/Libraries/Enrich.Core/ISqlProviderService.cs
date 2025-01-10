using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Enrich.Core
{
    /// <summary>
    /// Using for excute query, store procedure
    /// </summary>
    public interface ISqlProviderService
    {
        /// <summary>
        /// Excute sql query 
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="commandType"></param>
        /// <param name="param"></param>
        /// <returns>DataTable</returns>
        DataTable ExcuteDataTable(string SQL, CommandType commandType, params SqlParameter[] param);


        /// <summary>
        /// Excute sql query 
        /// </summary>
        /// <param name="query">query sql</param>
        /// <param name="commandType"></param>
        /// <returns>DataTable</returns>
        DataTable ExcuteDataTable(string query, CommandType commandType);

        /// <summary>
        /// ExcuteDataReader
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="commanType"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        SqlDataReader ExcuteDataReader(string SQL, CommandType commanType, params SqlParameter[] param);

        /// <summary>
        /// ExcuteDataReader
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="commanType"></param>
        /// <returns></returns>
        SqlDataReader ExcuteDataReader(string SQL, CommandType commanType);

        /// <summary>
        /// ExcuteNoneQuery
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="commandType"></param>
        /// <param name="param"></param>
        void ExcuteNoneQuery(string SQL, CommandType commandType, params SqlParameter[] param);

        /// <summary>
        /// ExcuteNoneQueryOutputParameter
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="commandType"></param>
        /// <param name="param"></param>
        string ExcuteQueryWithOutputParameter(string SQL, CommandType commandType, params SqlParameter[] param);

        /// <summary>
        /// Excute sql query 
        /// </summary>
        /// <param name="query">query sql</param>
        /// <returns>interger number</returns>
        int ExecuteQuery(string query);

        /// <summary>
        /// Execute StoredProcedure
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">query sql</param>
        /// <param name="parameters"></param>
        /// <returns>IEnumerable </returns>
        IEnumerable<T> ExecuteStoredProcedureList<T>(string query, params object[] parameters)
            where T : new();

        /// <summary>
        /// ExecuteQuery
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        IEnumerable<T> ExecuteQuery<T>(string query) where T : new();

        /// <summary>
        /// ExecuteQuery
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IEnumerable<T> ExecuteQuery<T>(string query, params object[] parameters) where T : new();


    }
}
