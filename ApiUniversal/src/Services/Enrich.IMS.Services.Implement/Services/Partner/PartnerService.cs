using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Enums;
using Enrich.IMS.Dto.Lookup;
using Enrich.IMS.Dto.Member;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class PartnerService : EnrichBaseService<Partner, PartnerDto>, IPartnerService
    {
        #region field
        private readonly EnrichContext _context;
        private IMemberMapper _mapper => _mapperGeneric as IMemberMapper;
        private readonly IPartnerRepository _repository;
        #endregion

        #region contructor
        public PartnerService(IPartnerRepository repository, IMemberMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
        }
        #endregion

        /// <summary>
        /// Optimize request for get lookup data
        /// </summary>
        /// <param name="request"></param>
        public void OptimizeConditionRequest(LookupDataRequest request, LookupDataType type)
        {
            request.FieldNames.Add(new SqlMapDto("Code", "Value"));
            request.FieldNames.Add(new SqlMapDto("Name", "Name"));
            request.ExtendConditions = $"[Status] = 1";
        }
    }
}