using Promotion.Model.Model.Comon;
using System.Data;
using System.Data.SqlClient;

namespace Promotion.Application.Extensions
{
    public class IMSEntityService<T> : EntityService<T> where T : class
    {
        public IMSEntityService()
        {
            _connection = new SqlConnection(Const.IMS_CONNECTION_STRING);
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
            //SimpleCRUD.SetDialect(SimpleCRUD.Dialect.SQLServer);
        }
        public IMSEntityService(IDbConnection db)
        {
            _connection = db;
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
            //SimpleCRUD.SetDialect(SimpleCRUD.Dialect.SQLServer);
        }
    }
}
