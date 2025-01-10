using Enrich.Common.Enums;
using Enrich.Core.Infrastructure.Repository;
using Enrich.Core.Infrastructure.Services;
using Enrich.Dto.Base;
using Enrich.Dto.Base.Exceptions;
using Enrich.Dto.Base.Requests;
using Enrich.Dto.Base.Responses;
using Enrich.Dto.List;
using Enrich.Dto.Requests;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Lookup;
using Enrich.IMS.Infrastructure.Data.Interface;
using Enrich.Services.Interface.Mappers;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Services.Implement
{
    public abstract class GenericService<T, TDto> : BaseService, IGenericService<T, TDto> where TDto : class
    {
        protected readonly IGenericRepository<T> _repositoryGeneric;
        protected readonly IBaseMapper _mapperGeneric;

        private readonly static ConcurrentDictionary<object, TDto> cache = new ConcurrentDictionary<object, TDto>();

        protected GenericService(IGenericRepository<T> genericRepository)
        {
            _repositoryGeneric = genericRepository;
        }

        protected GenericService(IGenericRepository<T> genericRepository, IBaseMapper mapper)
            : this(genericRepository)
        {
            _mapperGeneric = mapper;
        }
     
        public virtual int Delete(T item) => _repositoryGeneric.Delete(item);

        public virtual async Task<int> DeleteAsync(T item) => await _repositoryGeneric.DeleteAsync(item);

        public virtual int DeleteById(object id) => _repositoryGeneric.DeleteById(id);

        public virtual async Task<int> DeleteByIdAsync(object id) => await _repositoryGeneric.DeleteByIdAsync(id);

        public virtual TDto Get(object id, Language lanuage = Language.EN)
        {
            var entity = _repositoryGeneric.FindById(id);
            return Map(entity, lanuage);
        }

        public virtual async Task<TDto> GetAsync(object id, Language lanuage = Language.EN)
        {
            var entity = await _repositoryGeneric.FindByIdAsync(id);
            return Map(entity, lanuage);
        }

        public virtual TDto GetFromCache(object id, Language language = Language.EN)
        {
            if (id == null) return null;

            TDto dto = null;
            if (cache.TryGetValue(id, out dto) == false)
            {
                dto = Get(id, language);
                cache.TryAdd(id, dto);
            }

            return dto;
        }

        public TDto Add(TDto obj)
        {
            var entity = _mapperGeneric.Map<T>(obj);
            var result = _repositoryGeneric.AddGet(entity);
            return _mapperGeneric.Map<TDto>(result);
        }

        public async Task<object> AddAsync(TDto obj)
        {
            var entity = _mapperGeneric.Map<T>(obj);
            return await _repositoryGeneric.AddAsync(entity);
        }

        public async Task<TDto> AddGetAsync(TDto obj)
        {
            var entity = _mapperGeneric.Map<T>(obj);
            var result = await _repositoryGeneric.AddGetAsync(entity);

            return _mapperGeneric.Map<TDto>(result);
        }

        public int Update(TDto item)
        {
            var entity = _mapperGeneric.Map<T>(item);
            return _repositoryGeneric.Update(entity);

        }

        public async Task<int> UpdateAsync(TDto item)
        {
            var entity = _mapperGeneric.Map<T>(item);
            return await _repositoryGeneric.UpdateAsync(entity);
        }

        public int Delete(TDto item)
        {
            var entity = _mapperGeneric.Map<T>(item);
            return Delete(entity);
        }

        public virtual async Task<int> DeleteAsync(TDto item)
        {
            var entity = _mapperGeneric.Map<T>(item);
            return await DeleteAsync(entity);
        }

        public bool Exist(object id)
        {
            var obj = _repositoryGeneric.FindById(id);
            return obj != null;
        }

        public async Task<bool> ExistAsync(object id) => await _repositoryGeneric.IsExistsAsync(id);

        public async Task<PagingResponseDto<TDto>> QueryPagingAsync(QueryPagingRequest queryRequest, Language lanuage = Language.EN)
        {
            var queryParameter = _mapperGeneric.CreateSqlQueryParameter(queryRequest);
            var pagingEntities = await _repositoryGeneric.QueryPagingAsync<T>(queryParameter);

            return new PagingResponseDto<TDto>()
            {
                Pagination = pagingEntities.Pagination,
                Records = MapList(pagingEntities.Records)
            };
        }

        public virtual async Task<IEnumerable<TDto>> QueryAsync<TRequest>(TRequest request) where TRequest : QueryRequest, new()
        {
            var entities = await _repositoryGeneric.QueryAsync(request ?? new TRequest());

            return _mapperGeneric.Map<IEnumerable<TDto>>(entities);
        }

        public virtual IEnumerable<TDto> GetAll()
        {
            var entities = _repositoryGeneric.GetAll();
            return MapList(entities);
        }

        public virtual async Task<IEnumerable<TDto>> GetAllAsync()
        {
            var entities = await _repositoryGeneric.GetAllAsync();

            return MapList(entities);
        }

        public TDto Get(FindByIdRequest request)
        {
            var findByIdParameter = _mapperGeneric.CreateFindByIdParameter(request);
            var entity = _repositoryGeneric.FindById<T>(findByIdParameter);
            return Map(entity);
        }

        public Task<IEnumerable<TDto>> GetByIds(int[] ids)
        {
            return _repositoryGeneric.FindByIds<TDto>(ids);
        }
        protected virtual TDto Map(T entity, Language language = Language.EN)
        {
            var dto = _mapperGeneric.Map<TDto>(entity);

            return dto;
        }

        protected virtual IEnumerable<TDto> MapList(IEnumerable<T> entities, Language language = Language.EN)
        {
            return entities.Select(entity => Map(entity, language));
        }

        public int Update<AnyDto>(AnyDto item)
        {
            var entity = _mapperGeneric.Map<T>(item);
            return _repositoryGeneric.Update(entity);
        }

        public AnyDto Get<AnyDto>(object id, Language lanuage = Language.EN)
        {
            var entity = _repositoryGeneric.FindById(id);
            return _mapperGeneric.Map<AnyDto>(entity);
        }

        public async Task<AnyDto> GetAsync<AnyDto>(object id, Language lanuage = Language.EN)
        {
            var entity = await _repositoryGeneric.FindByIdAsync(id);
            return _mapperGeneric.Map<AnyDto>(entity);
        }

        public virtual Task<object> GetConfigsAsync()
        {
            return null;
        }

        #region ILookupDataService

        public virtual async Task<IEnumerable<IdNameDto>> GetIdNamesAsync(LookupDataRequest request)
        {
            if (_repositoryGeneric is ILookupDataRepository repLookup)
            {
                return await repLookup.GetIdNamesAsync(request);
            }

            return Enumerable.Empty<IdNameDto>();
        }

        public virtual async Task<IEnumerable<BaseEvent<TValue>>> GetBusinessEventAsync<TValue>(BusinessEventRequest request)
        {
            if (_repositoryGeneric is IBaseEventRepository repBusinessEvent)
            {
                var _events = await repBusinessEvent.GetEventAsync<TValue>(request);
                foreach (var ev in _events)
                {
                    if (!string.IsNullOrEmpty(ev.MetaData))
                    {
                        ev.Value = JsonConvert.DeserializeObject<TValue>(ev.MetaData);
                    }
                }
                return _events;
            }
            return Enumerable.Empty<BaseEvent<TValue>>();
        }

        #endregion


        #region IDeleteService

        public virtual async Task<DeleteResponse> DeleteAsync(DeleteRequest request)
        {
           throw new NotImplementedException();
        }

        public virtual async Task<DeleteResponse> RestoreAsync(DeleteRequest request)
        {
            if (request.RequestType != 1 && request.RequestType != 4)
            {
                return new DeleteResponse();
            }

            // depend on request.RequestType will determine delete/restore, if have special case -> override at parent-service
            return await DeleteAsync(request);
        }

        #endregion

        #region Exception

        public void Throw(EnrichException exception)
            => EnrichExceptionUtils.Throw(exception);

        public void ThrowValidations(params EnrichValidationFailure[] failures)
            => EnrichExceptionUtils.ThrowValidations(new List<EnrichValidationFailure>(failures), genericMsg: "ValidationExceptions");

        public void ThrowValidations(List<EnrichValidationFailure> failures, string genericMsg = "ValidationExceptions")
            => EnrichExceptionUtils.ThrowValidations(failures, genericMsg);

        public void ThrowMultiple(EnrichException[] exceptions, string genericMsg = "MultipleExceptions")
            => EnrichExceptionUtils.ThrowMultiple(exceptions, genericMsg);

        public EnrichException Throw(int code, string message, object extendData = null, int? httpStatusCode = null)
            => EnrichExceptionUtils.Throw(code, message, extendData, httpStatusCode);

        public EnrichException ThrowWithInner(int code, string message, Exception innerException, object extendData = null)
            => EnrichExceptionUtils.ThrowWithInner(code, message, innerException, extendData);
        public EnrichException ThrowWithOutNotify(int code, string message, object extendData = null, int? httpStatusCode = null, bool postBug = true, bool logError = true)
            => EnrichExceptionUtils.ThrowWithoutNotify(code, message, extendData, httpStatusCode, postBug, logError);


        #endregion

    }


}
