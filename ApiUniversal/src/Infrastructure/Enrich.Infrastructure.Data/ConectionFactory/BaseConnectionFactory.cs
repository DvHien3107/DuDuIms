using System;
using System.Data;
using System.Data.SqlClient;

namespace Enrich.Infrastructure.Data.ConectionFactory
{
    public class BaseConnectionFactory
    {
        protected string _connectionString;

        public virtual string ConnectionString => _connectionString;

        public BaseConnectionFactory()
        {
        }

        public BaseConnectionFactory(string connectionString)
            => _connectionString = connectionString;

        public virtual IDbConnection GetDbConnection()
            => GetDbConnection(_connectionString);

        public virtual IDbConnection GetDbConnection(string connectionString)
            => new SqlConnection(connectionString);

        #region IDisposable

        //https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
        // Flag: Has Dispose already been called?
        bool disposed = false;


        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }


            disposed = true;
        }

        ~BaseConnectionFactory()
        {
            Dispose(false);
        }

        #endregion
    }
}
