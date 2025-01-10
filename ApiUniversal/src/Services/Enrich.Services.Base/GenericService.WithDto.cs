using Enrich.Common.Enums;
using Enrich.Core.Infrastructure.Repository;
using Enrich.Core.Infrastructure.Services;
using Enrich.Dto.Requests;
using Enrich.Services.Interface.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Services.Base
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

        public async Task<IEnumerable<TDto>> GetDataChangesAsync(string connectionString, DateTimeOffset? minChangedDate)
        {
            var dataChanges = await _repositoryGeneric.GetDataChangesAsync(connectionString, minChangedDate);
            return MapList(dataChanges);
        }

        public async Task<TDto> GetDataChangeAsync(string connectionString, DateTimeOffset? changedDate)
        {
            var dataChange = await _repositoryGeneric.GetDataChangeAsync(connectionString, changedDate);
            return Map(dataChange);
        }

        public virtual int Delete(T item) => _repositoryGeneric.Delete(item);

        public virtual async Task<int> DeleteAsync(T item) => await _repositoryGeneric.DeleteAsync(item);

        public virtual int DeleteById(object id) => _repositoryGeneric.DeleteById(id);

        public virtual async Task<int> DeleteByIdAsync(object id) => await _repositoryGeneric.DeleteByIdAsync(id);

        public virtual async Task RestoresAsync(int[] ids) => await _repositoryGeneric.RestoresAsync(ids);

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

        public async Task<PagingResponseDto<TDto>> QueryPagingAsync(QueryPagingRequest queryRequest, Language lanuage = Language.NL)
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
        protected virtual TDto Map(T entity, Language language = Language.NL)
        {
            var dto = _mapperGeneric.Map<TDto>(entity);

            return dto;
        }

        protected virtual IEnumerable<TDto> MapList(IEnumerable<T> entities, Language language = Language.NL)
        {
            return entities.Select(entity => Map(entity, language));
        }

        public int Update<AnyDto>(AnyDto item)
        {
            var entity = _mapperGeneric.Map<T>(item);
            return _repositoryGeneric.Update(entity);
        }

        public AnyDto Get<AnyDto>(object id, Language lanuage = Language.NL)
        {
            var entity = _repositoryGeneric.FindById(id);
            return _mapperGeneric.Map<AnyDto>(entity);
        }

        public async Task<AnyDto> GetAsync<AnyDto>(object id, Language lanuage = Language.NL)
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

        #endregion

        #region IDeleteService

        public virtual async Task<DeleteResponse> DeleteAsync(DeleteRequest request)
        {
            if (_repositoryGeneric is IDeleteRepository repDelete)
            {
                return await repDelete.DeleteAsync(request);
            }

            return new DeleteResponse();
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

        #region IConfigTableService

        public virtual async Task<ConfigTableDto<TDto>> LoadConfigLookupAsync()
        {
            return new ConfigTableDto<TDto>
            {
                Records = (await GetAllAsync()).ToList()
            };
        }

        public virtual async Task SaveConfigLookupAsync(ConfigTableDto<TDto> tableDto)
        {
            if (typeof(IItemState).IsAssignableFrom(typeof(TDto)))
            {
                var changes = new ChangeEntity<T>
                {
                    Added = _mapperGeneric.Map<IEnumerable<T>>(tableDto.Records.Where(a => (a as IItemState).ItemState == ItemStateType.Added)).ToList(),
                    Modified = _mapperGeneric.Map<IEnumerable<T>>(tableDto.Records.Where(a => (a as IItemState).ItemState == ItemStateType.Modified)).ToList(),
                    Deleted = tableDto.Records.Where(a => (a as IItemState).ItemState == ItemStateType.Deleted).Select(a => (a as IItemState).Id).ToList()
                };

                await _repositoryGeneric.SaveChangesAsync(changes);
            }
        }

        #endregion
    }

    public abstract class BaseService
    {
        #region Changes

        public virtual ChangeEntity<TEntity> GetChangeEntities<TDto, TEntity>(IEnumerable<TDto> sources, IBaseMapper mapper) where TDto : ItemStateDto
        {
            var changes = GetChanges(sources);

            return new ChangeEntity<TEntity>
            {
                Added = mapper.Map<IEnumerable<TEntity>>(changes.Added).ToList(),
                Modified = mapper.Map<IEnumerable<TEntity>>(changes.Modified).ToList(),
                Deleted = changes.Deleted.Select(a => a.Id).ToList()
            };
        }

        public virtual (List<TDto> Added, List<TDto> Modified, List<TDto> Deleted) GetChanges<TDto>(IEnumerable<TDto> sources) where TDto : ItemStateDto
        {
            return
            (
                Added: sources.Added().ToList(),
                Modified: sources.Modified().ToList(),
                Deleted: sources.Deleted().ToList()
            );
        }

        #endregion

        #region Exception

        public void Throw(OmniException exception)
            => OmniExceptionUtils.Throw(exception);

        public void ThrowValidations(params OmniValidationFailure[] failures)
            => OmniExceptionUtils.ThrowValidations(new List<OmniValidationFailure>(failures), genericMsg: "ValidationExceptions");

        public void ThrowValidations(List<OmniValidationFailure> failures, string genericMsg = "ValidationExceptions")
            => OmniExceptionUtils.ThrowValidations(failures, genericMsg);

        public void ThrowMultiple(OmniException[] exceptions, string genericMsg = "MultipleExceptions")
            => OmniExceptionUtils.ThrowMultiple(exceptions, genericMsg);

        public OmniException Throw(int code, string message, object extendData = null, int? httpStatusCode = null)
            => OmniExceptionUtils.Throw(code, message, extendData, httpStatusCode);

        public OmniException ThrowWithInner(int code, string message, Exception innerException, object extendData = null)
            => OmniExceptionUtils.ThrowWithInner(code, message, innerException, extendData);
        public OmniException ThrowWithOutNotify(int code, string message, object extendData = null, int? httpStatusCode = null, bool postBug = true, bool logError = true)
            => OmniExceptionUtils.ThrowWithoutNotify(code, message, extendData, httpStatusCode, postBug, logError);


        #endregion
    }
}
