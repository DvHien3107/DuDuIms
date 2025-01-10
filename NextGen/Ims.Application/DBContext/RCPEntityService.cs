using Promotion.Application.Extensions;
using Promotion.Model.Model.Comon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.DBContext
{

    public class RCPEntityService<T> : EntityService<T> where T : class
    {
        public RCPEntityService()
        {
            _connection = new SqlConnection(Const.RCP_CONNECTION_STRING);
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
            //SimpleCRUD.SetDialect(SimpleCRUD.Dialect.SQLServer);
        }
        public RCPEntityService(IDbConnection db)
        {
            _connection = db;
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
            //SimpleCRUD.SetDialect(SimpleCRUD.Dialect.SQLServer);
        }
    }
}
