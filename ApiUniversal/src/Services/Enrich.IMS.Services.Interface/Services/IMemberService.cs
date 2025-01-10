using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Member;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface IMemberService : IGenericService<Member, MemberDto>, ILookupDataService
    {   
        /// <summary>
        /// quick search sales lead
        /// </summary>
        /// <param name="text">search texy</param>
        /// <returns>IEnumerable<SalesLeadQuickSearchItemDto></returns>
        Task<IEnumerable<MemberQuickSearchItemDto>> QuickSearchAsync(string text);
        Task<IEnumerable<MemberOptionItemDto>> GetSalesMemberAsync();

        Task<Member> GetByEmailAsync(string email);
    }
}