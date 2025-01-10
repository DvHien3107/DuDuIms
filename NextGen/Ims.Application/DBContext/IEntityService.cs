using Promotion.Model.Model.Comon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Promotion.Application.Interfaces
{
    public partial interface IEntityService<T> : IDisposable where T : class
    {
        void Execute(string sql, object param = null);
        Task ExecuteAsync(string sql, object param = null);
        IEnumerable<T> Query(string sql, object param = null);
        Task<IEnumerable<T>> QueryAsync(string sql, object param = null);
        Task<T> Get(object id);
        Task<T> Find(string condition, object parms = null);
        Task<IEnumerable<T>> GetAll(string condition = null, object parms = null, string orderBy = "", int? page = null, int? limit = Const.Limit);
        Task<IEnumerable<T>> GetAll(string select, string from, string condition = null, object parms = null, string orderBy = "", int? page = null, int? limit = Const.Limit);
        Task<int> Count(string from = "", string condition = null, object parms = null);
        long Inserts(T parms, IDbTransaction transaction = null);
        bool InsertMany(List<T> parms, IDbTransaction transaction = null);

        bool Updates(T parms, IDbTransaction transaction = null);

        bool Deactivate(object id);
        Task<bool> DeactivateAsync(object id);
        bool Delete(object id);
        Task<bool> DeleteAsync(object id);

    }
}
