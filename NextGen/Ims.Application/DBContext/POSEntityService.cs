using Promotion.Model.Model.Comon;
using System.Data;
using System.Data.SqlClient;

namespace Promotion.Application.Extensions
{
    public class POSEntityService<T> : EntityService<T> where T : class
    {
        public POSEntityService()
        {
            _connection = new SqlConnection(Const.POS_CONNECTION_STRING);
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
            //SimpleCRUD.SetDialect(SimpleCRUD.Dialect.SQLServer);
        }
        public POSEntityService(IDbConnection db)
        {
            _connection = db;
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
            //SimpleCRUD.SetDialect(SimpleCRUD.Dialect.SQLServer);
        }
    }
}
