using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.Data
{
    public interface IConnectionFactory
    {
        string ConnectionString { get; }

        IDbConnection GetDbConnection();

        IDbConnection GetDbConnection(string connectionString);

        #region Shared

        IDbConnection SharedConnection { get; set; }

        IDbTransaction SharedTransaction { get; set; }

        bool IsSharedConnection { get; }

        #endregion
    }
}
