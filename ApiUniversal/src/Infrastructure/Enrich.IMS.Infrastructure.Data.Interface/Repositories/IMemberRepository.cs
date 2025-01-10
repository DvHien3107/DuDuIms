using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Dto.Member;
using Enrich.IMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface IMemberRepository : IGenericRepository<Member>, ILookupDataRepository
    {
        /// <summary>
        /// get member by email dang password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Task<Member> GetMemberByEmailAndPassword(string email, string password);

        Task<Member> GetMemberBySecretKey(string secretKey);

        /// <summary>
        /// quick search member
        /// </summary>
        /// <param name="text"> search text</param>
        /// <returns>IEnumerable<MemberQuickSearchItemDto></returns>
        Task<IEnumerable<MemberQuickSearchItemDto>> QuickSearchAsync(string text);

        Task<IEnumerable<MemberOptionItemDto>> GetSalesMemberAsync();

        Task<Member> GetByEmailAsync(string email);
    }
}