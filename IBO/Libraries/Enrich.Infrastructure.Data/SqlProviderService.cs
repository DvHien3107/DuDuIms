using Enrich.Core;
using Enrich.Core.UnitOfWork.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.Data
{
    public partial class SqlProviderService : ISqlProviderService
    {
        private readonly IUnitOfWork _unitOfWork;

        #region contructor
        //public SqlProviderService()
        //{
        //    //_unitOfWork = new UnitOfWork();
        //}
        public SqlProviderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #endregion

        public int ExecuteQuery(string query)
        {
            return _unitOfWork.ExecuteQuery(query);
        }

        public IEnumerable<T> ExecuteStoredProcedureList<T>(string query, params object[] parameters) where T : new()
        {
            return _unitOfWork.ExecuteStoredProcedureList<T>(query, parameters);
        }

        public IEnumerable<T> ExecuteQuery<T>(string query, params object[] parameters) where T : new()
        {
            return _unitOfWork.ExecuteStoredProcedureList<T>(query, parameters);
        }

        public IEnumerable<T> ExecuteQuery<T>(string query) where T : new()
        {
            return _unitOfWork.ExecuteStoredProcedureList<T>(query);
        }
    }


    public partial class SqlProviderService
    {

        /// <summary>
        /// Get connectionstring
        /// </summary>
        /// <returns></returns>
        private SqlConnection getSqlConnection()
        {
            SqlConnection cnn = new SqlConnection(EnrichObjectContext.GetConectionString());
            if (cnn.State == ConnectionState.Closed)
                cnn.Open();
            return cnn;
        }

        /// <summary>
        /// excute sql query, which have SqlParameter
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="commandType"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public DataTable ExcuteDataTable(string SQL, CommandType commandType, params SqlParameter[] parameters)
        {


            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            using (SqlConnection cnn = getSqlConnection())
            {
                cmd.Connection = cnn;
                cmd.CommandText = SQL;
                cmd.CommandType = commandType;
                foreach (SqlParameter par in parameters)
                {
                    par.ParameterName = par.ParameterName.Trim();
                    if (!par.ParameterName.StartsWith("@"))
                    {
                        par.ParameterName = "@" + par.ParameterName;
                    }
                    cmd.Parameters.Add(par);
                }
                try
                {
                    dt.Load(cmd.ExecuteReader());
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return dt;
        }

        /// <summary>
        /// excute sql query, which dont have SqlParameter
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>

        public DataTable ExcuteDataTable(string SQL, CommandType commandType)
        {
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            using (SqlConnection cnn = getSqlConnection())
            {
                cmd.Connection = cnn;
                cmd.CommandText = SQL;
                cmd.CommandType = commandType;
                try
                {
                    dt.Load(cmd.ExecuteReader());
                }
                catch (Exception ex)
                {

                    throw new Exception(ex.Message);
                }
            }

            return dt;
        }

        /// <summary>
        /// excute sql query return SqlDataReader
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="commanType"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public SqlDataReader ExcuteDataReader(string SQL, CommandType commanType, params SqlParameter[] param)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader = null;
            var cnn = getSqlConnection();

            cmd.Connection = cnn;
            cmd.CommandText = SQL;
            cmd.CommandType = commanType;
            foreach (SqlParameter par in param)
            {
                par.ParameterName = par.ParameterName.Trim();
                if (!par.ParameterName.StartsWith("@"))
                {
                    par.ParameterName = "@" + par.ParameterName;
                }

                cmd.Parameters.Add(par);
            }
            //cmd.Parameters.AddRange(param);

            try
            {
                reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {

            }
            return reader;
        }

        /// <summary>
        /// excute sql query return SqlDataReader
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="commanType"></param>
        /// <returns></returns>
        public SqlDataReader ExcuteDataReader(string SQL, CommandType commanType)
        {

            SqlDataReader reader = null;
            var cnn = getSqlConnection();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cnn;
            cmd.CommandText = SQL;
            cmd.CommandType = commanType;
            try
            {
                reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


            return reader;
        }

        /// <summary>
        /// excute sql query, exp: insert, update...
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="commandType"></param>
        /// <param name="param"></param>
        public void ExcuteNoneQuery(string SQL, CommandType commandType, params SqlParameter[] param)
        {
            using (SqlConnection cnn = getSqlConnection())
            {

                SqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = SQL;
                cmd.CommandType = commandType;
                foreach (SqlParameter par in param)
                {
                    par.ParameterName = par.ParameterName.Trim();
                    if (!par.ParameterName.StartsWith("@"))
                    {
                        par.ParameterName = "@" + par.ParameterName;
                    }

                    cmd.Parameters.Add(par);
                }
                //cmd.Parameters.AddRange(param);

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        /// <summary>
        /// excute sql query, exp: insert, update...
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="commandType"></param>
        /// <param name="param"></param>
        public string ExcuteQueryWithOutputParameter(string SQL, CommandType commandType, params SqlParameter[] param)
        {
            using (SqlConnection cnn = getSqlConnection())
            {
                SqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = SQL;
                cmd.CommandType = commandType;

                var paraOutPut = "";
                foreach (SqlParameter par in param)
                {
                    par.ParameterName = par.ParameterName.Trim();
                    if (!par.ParameterName.StartsWith("@"))
                    {
                        par.ParameterName = "@" + par.ParameterName;
                    }

                    cmd.Parameters.Add(par);
                    if (par.Direction == ParameterDirection.Output)
                    {
                        paraOutPut = par.ParameterName;
                    }
                }
                //cmd.Parameters.AddRange(param);

                try
                {
                    cmd.ExecuteNonQuery();
                    if (paraOutPut != "")
                    {
                        return cmd.Parameters[paraOutPut].Value.ToString();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            return "";
        }

        /// <summary>
        /// excute sql query, exp: insert, update...
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="commandType"></param>
        /// <param name="param"></param>
        public Dictionary<string, string> ExcuteQueryWithMultipleOutputParameter(string SQL, CommandType commandType, params SqlParameter[] param)
        {
            using (SqlConnection cnn = getSqlConnection())
            {
                SqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = SQL;
                cmd.CommandType = commandType;

                var paraOutPut = new List<string>();
                var paraOutPut1 = new Dictionary<string, string>();
                foreach (SqlParameter par in param)
                {
                    par.ParameterName = par.ParameterName.Trim();
                    if (!par.ParameterName.StartsWith("@"))
                    {
                        par.ParameterName = "@" + par.ParameterName;
                    }

                    cmd.Parameters.Add(par);
                    if (par.Direction == ParameterDirection.Output)
                    {
                        paraOutPut.Add(par.ParameterName);
                    }
                }
                //cmd.Parameters.AddRange(param);

                try
                {
                    cmd.ExecuteNonQuery();
                    foreach (var ou in paraOutPut)
                    {
                        paraOutPut1.Add(ou, cmd.Parameters[ou].Value.ToString());
                    }

                    return paraOutPut1;
                    //if (paraOutPut != "")
                    //{
                    //    return paraOutPut.;//cmd.Parameters[paraOutPut].Value.ToString();
                    //}
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
