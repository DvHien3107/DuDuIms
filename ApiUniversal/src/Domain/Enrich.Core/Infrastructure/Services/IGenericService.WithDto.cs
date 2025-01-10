using Enrich.Common.Enums;
using Enrich.Dto.List;
using Enrich.Dto.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Core.Infrastructure.Services
{
    public interface IGenericService<T, TDto> : IService
    {
        TDto Add(TDto obj);

        Task<TDto> AddGetAsync(TDto obj);

        Task<object> AddAsync(TDto obj);

        int Update(TDto item);

        Task<int> UpdateAsync(TDto item);

        int Update<AnyDto>(AnyDto item);

        int Delete(TDto item);

        Task<int> DeleteAsync(TDto item);

        int DeleteById(object id);

        Task<int> DeleteByIdAsync(object id);

        TDto Get(object id, Language language = Language.EN);

        Task<TDto> GetAsync(object id, Language lanuage = Language.EN);

        TDto GetFromCache(object id, Language language = Language.EN);

        AnyDto Get<AnyDto>(object id, Language lanuage = Language.EN);

        Task<AnyDto> GetAsync<AnyDto>(object id, Language lanuage = Language.EN);

        TDto Get(FindByIdRequest request);

        Task<IEnumerable<TDto>> GetByIds(int[] ids);

        bool Exist(object id);

        Task<bool> ExistAsync(object id);

        IEnumerable<TDto> GetAll();

        Task<IEnumerable<TDto>> GetAllAsync();

        Task<PagingResponseDto<TDto>> QueryPagingAsync(QueryPagingRequest queryRequest, Language lanuage = Language.EN);

        Task<IEnumerable<TDto>> QueryAsync<TRequest>(TRequest request) where TRequest : QueryRequest, new();

    }
}
