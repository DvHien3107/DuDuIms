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
    public partial class MemberService : EnrichBaseService<Member, MemberDto>, IMemberService
    {
        #region field
        private readonly EnrichContext _context;
        private IMemberMapper _mapper => _mapperGeneric as IMemberMapper;
        private readonly IMemberRepository _repository;
        #endregion

        #region contructor
        public MemberService(IMemberRepository repository, IMemberMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
        }
        #endregion

        #region Search

        /// <summary>
        /// quick search member
        /// </summary>
        /// <param name="text">search texy</param>
        /// <returns>IEnumerable<MemberQuickSearchItemDto></returns>
        public async Task<IEnumerable<MemberQuickSearchItemDto>> QuickSearchAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Enumerable.Empty<MemberQuickSearchItemDto>();
            }

            return await _repository.QuickSearchAsync(text);
        }


        public async Task<IEnumerable<MemberOptionItemDto>> GetSalesMemberAsync()
        {
            return await _repository.GetSalesMemberAsync();
        }

        public async Task<Member> GetByEmailAsync(string email)
        {
            return await _repository.GetByEmailAsync(email);
        }

        #endregion


        /// <summary>
        /// Optimize request for get lookup data
        /// </summary>
        /// <param name="request"></param>
        public void OptimizeConditionRequest(LookupDataRequest request, LookupDataType type)
        {
            request.FieldNames.Add(new SqlMapDto("MemberNumber", "Value"));
            request.FieldNames.Add(new SqlMapDto("FullName", "Name"));
            switch (type)
            {
                case LookupDataType.SalesMember:
                    request.ExtendConditions = $"Active = 1 AND CHARINDEX('19120010', DepartmentId) > 0";
                    break;
                default:
                    request.ExtendConditions = $"Active = 1";
                    break;
            }
        }
    }
}
