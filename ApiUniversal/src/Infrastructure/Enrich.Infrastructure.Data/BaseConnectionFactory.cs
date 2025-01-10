using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.Data
{
    public abstract class BaseConnectionFactory : IDisposable
    {
        protected string _connectionString;

        public virtual string ConnectionString => _connectionString;

        public BaseConnectionFactory(string connectionString)
            => _connectionString = connectionString;

        public virtual IDbConnection GetDbConnection()
            => GetDbConnection(_connectionString);

        public virtual IDbConnection GetDbConnection(string connectionString)
            => new SqlConnection(connectionString);

        #region Shared

        public IDbConnection SharedConnection { get; set; }

        public IDbTransaction SharedTransaction { get; set; }

        public bool IsSharedConnection => SharedConnection != null && !string.IsNullOrWhiteSpace(SharedConnection.ConnectionString);

        #endregion

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

            // Free any unmanaged objects here.
            try
            {
                if (SharedTransaction != null)
                {
                    SharedTransaction.Dispose();
                    SharedTransaction = null;
                }
                if (SharedConnection != null)
                {
                    if (SharedConnection.State == ConnectionState.Open)
                        SharedConnection.Close();

                    SharedConnection.Dispose();
                    SharedConnection = null;
                }
            }
            catch
            {
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
