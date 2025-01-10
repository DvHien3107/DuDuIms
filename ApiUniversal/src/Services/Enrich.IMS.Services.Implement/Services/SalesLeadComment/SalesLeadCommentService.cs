using Enrich.Core.Container;
using Enrich.Dto;
using Enrich.Dto.Base;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Enums;
using Enrich.IMS.Dto.Lookup;
using Enrich.IMS.Dto.SalesLeadComment;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Builders;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Infrastructure.Data.Dapper.Library;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class SalesLeadCommentService : EnrichBaseService<SalesLeadComment, SalesLeadCommentDto>, ISalesLeadCommentService
    {
        private ISalesLeadCommentMapper _mapper => _mapperGeneric as ISalesLeadCommentMapper;
        private readonly EnrichContext _context;
        private readonly IEnrichContainer _container;
        private readonly ISalesLeadCommentBuilder _builder;
        private readonly ISalesLeadCommentRepository _repository;
        private readonly ISalesLeadRepository _salesLeadRepository;

        public SalesLeadCommentService(EnrichContext context,
            ISalesLeadCommentMapper mapper,
            ISalesLeadCommentRepository repository,
            IEnrichContainer container,
            ISalesLeadRepository salesLeadRepository,
            ISalesLeadCommentBuilder builder) : base(repository, mapper)
        {
            _context = context;
            _container = container;
            _repository = repository;
            _salesLeadRepository = salesLeadRepository;
            _builder = builder;
        }

        public async Task<SalesLeadCommentDto> GetByIdAsync(int id)
        {
            var entity = await _repository.FindByIdAsync(id);
            var dto = _mapper.Map<SalesLeadCommentDto>(entity);
            return dto;
        }

        public void OptimizeConditionRequest(LookupDataRequest request, LookupDataType type)
        {
        }

        /// <summary>
        /// Search sales lead comment. Use for data grid
        /// </summary>
        /// <param name="request">SalesLeadSearchRequest</param>
        /// <returns>SalesLeadSearchResponse</returns>
        public async Task<SalesLeadCommentSearchResponse> SearchAsync(SalesLeadCommentSearchRequest request)
        {
            request.SqlQueryParam = _mapper.CreateSearchSqlQueryParameter<SalesLeadCommentItemDto>(request);
            OptimizeConditionSearch(request);
            var response = await _repository.SearchAsync(request);
            // format data base on require
            PopulateItems(response);
            return response;
        }
        private void PopulateItems(SalesLeadCommentSearchResponse response)
        {
            if (response == null || response.Records == null) return;

            //customize data after get from responsitory
        }

        private void OptimizeConditionSearch(SalesLeadCommentSearchRequest request)
        {
            // optimize Condition
            if (request.Condition == null) request.Condition = new SalesLeadCommentSearchCondition();
            request.Condition.PopulateCountSummaries = false; // !queryParam.HasPaging() || queryParam.Paging.PageIndex == 1;

            // OnStringFields
            if (request.Condition.OnStringFields?.Count > 0)
            {
                request.Condition.OnStringFields.RemoveAll(a => string.IsNullOrWhiteSpace(a.Key) || string.IsNullOrWhiteSpace(a.Value));
            }
            if (request.Condition.OnStringFields?.Count > 0)
            {
                // convert dto-name to db-name
                var dbFields = FieldDbHelper.GetFields<SalesLeadDto>();
                request.Condition.OnStringFields = request.Condition.OnStringFields
                    .Select(a => new { FieldDb = dbFields.FirstOrDefault(b => b.Prop.Name.Equals(a.Key)), FieldDto = a })
                    .Where(a => a.FieldDb != null).Select(a => new KeyValueDto<string> { Key = a.FieldDb.Name, Value = a.FieldDto.Value }).ToList();
            }
        }
    }
}