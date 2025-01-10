using Enrich.Core.UnitOfWork;
using Enrich.Core.UnitOfWork.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbContext _context;

        private bool _disposed;
        private ObjectContext _objectContext;
        private Hashtable _repositories;
        private DbTransaction _transaction;
        public UnitOfWork(IDbContext context)
        {
            _context = context;

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                    _context.Dispose();

            _disposed = true;
        }



        /// <summary>
        /// call savechange in DataContext
        /// </summary>
        /// <returns></returns>
        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public IRepository<T> Repository<T>() where T : BaseEntity
        {
            if (_repositories == null)
                _repositories = new Hashtable();

            var type = typeof(T).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(EfRepository<>);

                var repositoryInstance =
                    Activator.CreateInstance(repositoryType
                            .MakeGenericType(typeof(T)), _context);

                _repositories.Add(type, repositoryInstance);
            }

            return (IRepository<T>)_repositories[type];
        }

        public IEnumerable<T> SelectQuery<T>(string query)
        {
            return _context.SqlQuery<T>(query);
        }

        public IEnumerable<T> ExecuteStoredProcedureList<T>(string query, params object[] parameters) where T : new()
        {
            return _context.ExecuteStoredProcedureList<T>(query, parameters);
        }

        public int ExecuteQuery(string query, params object[] parameters)
        {
            return _context.ExecuteSqlCommand(query, parameters: parameters);
        }

        public int ExecuteSqlCommand(string query)
        {
            return _context.ExecuteSqlCommand(query);
        }

        public DataTable GetDataTable(string sqlQuery)
        {
            try
            {
                DbProviderFactory factory = DbProviderFactories.GetFactory(_context.Database.Connection.ToString());

                using (var cmd = factory.CreateCommand())
                {
                    cmd.CommandText = sqlQuery;
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = _context.Database.Connection;
                    using (var adapter = factory.CreateDataAdapter())
                    {
                        adapter.SelectCommand = cmd;

                        var tb = new DataTable();
                        adapter.Fill(tb);
                        return tb;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            _objectContext = ((IObjectContextAdapter)_context).ObjectContext;
            if (_objectContext.Connection.State != ConnectionState.Open)
            {
                _objectContext.Connection.Open();
            }

            _transaction = _objectContext.Connection.BeginTransaction(isolationLevel);
        }

        public void Rollback()
        {
            _transaction.Rollback();

        }

        public bool Commit()
        {
            _transaction.Commit();
            return true;
        }
    }
}
