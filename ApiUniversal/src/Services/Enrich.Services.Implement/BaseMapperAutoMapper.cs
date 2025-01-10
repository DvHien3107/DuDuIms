using AutoMapper;
using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto;
using Enrich.Dto.Base.Parameters;
using Enrich.Dto.Parameters;
using Enrich.Dto.Requests;
using Enrich.Infrastructure.Data.Dapper;
using Enrich.Services.Implement.Library;
using Enrich.Services.Interface.Mappers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Services.Implement
{
    public abstract class BaseMapperAutoMapper<TDefaultSource, TDefaultDestination> : IBaseMapper
    {
        private IMapper _mapper;

        public Action<object, object> BeforeMap { set; get; }
        public Action<object, object> AfterMap { set; get; }

        private ConcurrentDictionary<string, SqlMapDto> dicColumnMapperByColumnNames = new ConcurrentDictionary<string, SqlMapDto>(StringComparer.OrdinalIgnoreCase);
        private ConcurrentDictionary<string, SqlMapDto> dicColumnMapperByColumnAlias = new ConcurrentDictionary<string, SqlMapDto>(StringComparer.OrdinalIgnoreCase);

        private ConcurrentDictionary<string, PropertyInfo> dicDestProperties = new ConcurrentDictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        private ConcurrentDictionary<string, PropertyInfo> dicSourceProperties = new ConcurrentDictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

        public BaseMapperAutoMapper()
        {
            SetMapper(new Mapper(CreateMapperConfiguration()));
        }

        public void SetMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public TDestination Map<TDestination>(object source)
        {
            return Map<TDestination>(source, BeforeMap, AfterMap);
        }

        public TDestination Map<TDestination>(object source, Action<TDestination> after = null)
        {
            var dest = Map<TDestination>(source);

            if (dest != null && after != null)
            {
                after(dest);
            }

            return dest;
        }

        public TDestination Map<TDestination>(object source, Action<object, object> beforeMap, Action<object, object> afterMap)
        {
            if (source == null)
            {
                return default(TDestination);
            }

            return _mapper.Map<TDestination>(source, opts =>
            {
                if (beforeMap != null)
                    opts.BeforeMap(beforeMap);
                if (afterMap != null)
                    opts.AfterMap(afterMap);
            });
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return _mapper.Map(source, destination);
        }

        protected IConfigurationProvider CreateMapperConfiguration()
        {
            return new MapperConfiguration(ConfigMapperProvider);
        }

        protected virtual void ConfigMapperProvider(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<TDefaultSource, TDefaultDestination>();
            cfg.CreateMap<TDefaultDestination, TDefaultSource>();
        }

        public object GetMapEngine()
        {
            return _mapper;
        }

        public string GetDestinationPropertyFor<TSrc, TDst>(MapperConfiguration mapper, string sourceProperty)
        {
            //var map = mapper.FindTypeMapFor<TSrc, TDst>();
            //var propertyMap = map.GetPropertyMaps().First(pm => pm.SourceMember == typeof(TSrc).(sourceProperty));

            //return propertyMap.DestinationProperty.Name;
            return string.Empty;
        }

        public IBaseMapper AddColumnMapper(string columnName, string columnAlias)
        {
            AddColumnMapper(new SqlMapDto(columnName, columnAlias));
            return this;
        }

        public IBaseMapper AddColumnMapper(SqlMapDto columnMapper)
        {
            dicColumnMapperByColumnNames.TryAdd(columnMapper.SqlName, columnMapper);
            dicColumnMapperByColumnAlias.TryAdd(columnMapper.DtoName, columnMapper);
            return this;
        }

        public SqlMapDto GetColumnMapperByColumnName(string columnName)
        {
            SqlMapDto mapper = new SqlMapDto(columnName, columnName);//default
            dicColumnMapperByColumnNames.TryGetValue(columnName, out mapper);
            return mapper;
        }

        public SqlMapDto GetColumnMapperByColumnAlias(string columnAlias)
        {
            SqlMapDto mapper = null;
            dicColumnMapperByColumnAlias.TryGetValue(columnAlias, out mapper);
            if (mapper == null)
            {
                if (dicSourceProperties.ContainsKey(columnAlias))//columnalias = columnName
                    mapper = new SqlMapDto(columnAlias, columnAlias);//default
            }

            return mapper;
        }

        public string GetAliasByColumnName(string columnName)
        {
            var mapper = GetColumnMapperByColumnName(columnName);
            return mapper.DtoName;
        }

        public string GetColumnNameByAlias(string columnAlias)
        {
            var mapper = GetColumnMapperByColumnAlias(columnAlias);
            return mapper.SqlName;
        }

        public virtual QueryParameter MapToQueryParameter(QueryPagingRequest queryRequest, QueryParameter parameter)
        {
            if (queryRequest == null) return parameter;

            var fields = ParseFields(queryRequest.Fields);
            List<SqlMapDto> lstColumnMapper = new List<SqlMapDto>();
            if (fields == null || fields.Count() == 0)//we need select all here
            {
                foreach (var pair in dicDestProperties)
                {
                    var columnMapper = GetColumnMapperByColumnAlias(pair.Key);
                    if (columnMapper != null)
                        lstColumnMapper.Add(columnMapper);
                }
                fields = lstColumnMapper;
            }

            parameter.Fields = fields;

            parameter.Paging = ParsePaging(queryRequest.PageIndex, queryRequest.PageSize);
            parameter.OrderBy = ParseOrderBy(queryRequest.OrderBy);

            return parameter;
        }

        public virtual QueryParameter CreateSqlQueryParameter(QueryPagingRequest queryRequest)
        {
            var parameter = new QueryParameter();
            if (queryRequest == null) return parameter;

            return MapToQueryParameter(queryRequest, parameter);
        }
        public FindByIdParameter CreateFindByIdParameter(FindByIdRequest findByIdRequest)
        {
            var parameter = new FindByIdParameter();
            if (findByIdRequest == null) return parameter;

            parameter.Id = findByIdRequest.Id;

            var fields = ParseFields(findByIdRequest.Fields);
            List<SqlMapDto> lstColumnMapper = new List<SqlMapDto>();
            if (fields == null || fields.Count() == 0)//we need select all here
            {
                foreach (var pair in dicDestProperties)
                {
                    var columnMapper = GetColumnMapperByColumnAlias(pair.Key);
                    if (columnMapper != null)
                        lstColumnMapper.Add(columnMapper);
                }
                fields = lstColumnMapper;
            }

            parameter.Fields = fields;
            return parameter;
        }

        protected virtual List<SqlMapDto> ParseFields(string fields)
        {
            if (string.IsNullOrWhiteSpace(fields)) return null;

            var arrFields = fields.Split(Constants.SEPARATOR_CHARACTERS, StringSplitOptions.RemoveEmptyEntries);
            List<SqlMapDto> lstColumnMappers = new List<SqlMapDto>();
            foreach (var fieldName in arrFields)
            {
                var columnMapper = GetColumnMapperByColumnAlias(fieldName.Trim());
                if (columnMapper != null)
                    lstColumnMappers.Add(columnMapper);
            }

            return lstColumnMappers;
        }

        protected virtual PagingParameter ParsePaging(int pageIndex, int pageSize)
        {
            if (pageIndex < 0 && pageSize < 0)
            {
                pageIndex = 1;
                pageSize = int.MaxValue; //no paging
            }
            else
            {
                if (pageIndex <= 0) pageIndex = 1;
                if (pageSize <= 0) pageSize = Constants.DEFAULT_PAGE_SIZE;
            }

            return new PagingParameter
            {
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        protected virtual List<OrderByParameter> ParseOrderBy(string orderbyFields)
        {
            if (string.IsNullOrWhiteSpace(orderbyFields))
                return null;

            var sortParams = new List<OrderByParameter>();

            var sortFieldNames = orderbyFields.SplitEx();
            foreach (var sortFieldName in sortFieldNames)
            {
                string fieldAlias = sortFieldName.Trim('-');
                var field = GetColumnMapperByColumnAlias(fieldAlias);
                if (field != null)
                    sortParams.Add(new OrderByParameter(sortFieldName.StartsWith("-") ? SortDirectionEnum.Descending : SortDirectionEnum.Ascending) { Field = field });
            }

            return sortParams;
        }

        public virtual QueryParameter CreateSearchSqlQueryParameter<TItem>(BaseSearchRequest request, string aliasFieldId = "Id")
        {
            var fields = GridSqlHelper.GetFields<TItem>();
            var defaultField = fields.FirstOrDefault(a => a.DtoName.EqualsEx(aliasFieldId));

            var queryParam = new QueryParameter
            {
                ViewMode = request.ViewMode,
                IsOnlyGetTotalRecords = request.IsOnlyGetTotalRecords,
                Fields = GridSqlHelper.GetSelectFields(fields, request.Fields, request.DisplayLanguage),
                Paging = ParsePaging(request.PageIndex, request.PageSize)
            };

            //select
            if (queryParam.Fields.Count == 0 && defaultField != null)
            {
                queryParam.Fields.Add(defaultField);
            }

            //orderby
            queryParam.OrderBy = GridSqlHelper.GetOrderFields(fields, request.OrderBy);
            if (queryParam.OrderBy.Count == 0 && defaultField != null)
            {
                queryParam.OrderBy.Add(new OrderByParameter(SortDirectionEnum.Descending) { Field = defaultField });
            }

            return queryParam;
        }
    }
}
