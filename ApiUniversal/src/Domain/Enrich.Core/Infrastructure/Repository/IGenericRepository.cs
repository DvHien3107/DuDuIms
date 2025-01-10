using Enrich.Dto.Base;
using Enrich.Dto.List;
using Enrich.Dto.Parameters;
using Enrich.Dto.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Core.Infrastructure.Repository
{
    public interface IGenericRepository<T> : IRepository
    {    

        T AddGet(T obj);
        Task<T> AddGetAsync(T obj);
        Task<object> AddAsync(T obj);
        Task BulkInsertAsync(IEnumerable<T> entities);

        int Delete(T obj);
        Task<int> DeleteAsync(T obj);
        int DeleteById(object id);
        Task<int> DeleteByIdAsync(object id);
        int Update(T obj);
        Task<int> UpdateAsync(T obj);
        T FindById(object id);

        Task<T> FindByIdAsync(object id);

        IEnumerable<T> GetByRawSql(string sql, params object[] parameters);
        IEnumerable<T> GetAll();
        Task<IEnumerable<T>> GetAllAsync();
        IEnumerable<T> GetListPaged(int pageNumber, int rowsPerPage, string conditions, string orderby, object parameters = null);
        IQueryable<T> FindBy(Expression<Func<T, bool>> predicate);
        object GetObjectId(T entity);
        void SetObjectId(T entity, object id);

        int GetTotalCount();

        Task<PagingResponseDto<TDto>> QueryPagingAsync<TDto>(QueryParameter parameter);
        Task<IEnumerable<T>> QueryAsync(QueryRequest request);

        TDto FindById<TDto>(object Id);

        Task<IEnumerable<TDto>> FindByIds<TDto>(int[] Ids);         
     

        Task<bool> SaveAsync(object id, T obj, bool? isNew = null);

        Task SaveChangesAsync(ChangeEntity<T> changes);

        Task<bool> IsExistsAsync(object id);
     
    }

    public interface IRepository
    {
        int Execute(string sql, object param = null);
        Task<int> ExecuteAsync(string sql, object param = null, bool usedShared = false);   

        #region Custom

        Task<object> AddAsync<TEntity>(TEntity obj) where TEntity : class;

        Task<int> UpdateAsync<TEntity>(TEntity obj) where TEntity : class;

        Task<bool> SaveAsync<TEntity>(object id, TEntity obj, bool? isNew = null) where TEntity : class;

        Task SaveChangesAsync<TEntity>(ChangeEntity<TEntity> changes);

        Task SaveChangesAsync(bool forEach, params (string Query, List<(string Key, object Value)> Parameters)[] changes);

        #endregion

    }
}
