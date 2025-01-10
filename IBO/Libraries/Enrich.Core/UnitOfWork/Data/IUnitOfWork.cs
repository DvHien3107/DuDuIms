using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Core.UnitOfWork.Data
{
    public interface IUnitOfWork : IDisposable
    {
        int SaveChanges();
        void Dispose(bool disposing);
        IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
        IEnumerable<T> SelectQuery<T>(string query);
        IEnumerable<T> ExecuteStoredProcedureList<T>(string query, params object[] parameters) where T : new(); 
        DataTable GetDataTable(string sqlQuery);
        int ExecuteQuery(string query, params object[] parameters);
        bool Commit();
        void Rollback();
    }
}
