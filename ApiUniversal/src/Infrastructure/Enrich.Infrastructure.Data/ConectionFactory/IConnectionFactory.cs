using System.Data;

namespace Enrich.Infrastructure.Data.ConectionFactory
{
    public interface IConnectionFactory
    {
        string ConnectionString { get; }
        IDbConnection GetDbConnection(string connectionString);

        IDbConnection GetDbConnection();

    }
}
