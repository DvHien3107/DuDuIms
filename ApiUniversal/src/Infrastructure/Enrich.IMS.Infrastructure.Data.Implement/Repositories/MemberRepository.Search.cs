using Dapper;
using Enrich.Common;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Member;
using Enrich.Infrastructure.Data.Dapper;
using Enrich.Infrastructure.Data.Dapper.Library;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class MemberRepository
    {
        /// <summary>
        /// Search member. Use for dropdownlist. Need to improve to full text search
        /// </summary>
        /// <param name="request">search text</param>
        /// <returns>MemberQuickSearchItemDto</returns>
        public async Task<IEnumerable<MemberQuickSearchItemDto>> QuickSearchAsync(string text)
        {
            var param = new DynamicParameters();
            string statementTOP = $"TOP({ Constants.DEFAULT_QUICK_SEARCH_ITEMS})";
            var searchFields = GridSqlHelper.GetFields<MemberQuickSearchItemDto>();
            var selectFields = GenerateSqlSelect(searchFields, includeKeywordSelect: false, includeKeyworkDistinct: false);

            var query = $"SELECT {statementTOP} {selectFields} " +
                        $"FROM {SqlTables.Member} " +
                        $"WHERE MemberNumber like @SearhText OR " +
                        $" FirstName like @SearhText OR" +
                        $" LastName like @SearhText OR" +
                        $" FullName like @SearhText OR" +
                        $" PersonalEmail like @SearhText";
            param.Add("SearhText", $"%{text}%");
            return await GetEnumerableAsync<MemberQuickSearchItemDto>(query, param);
        }

    }
}
