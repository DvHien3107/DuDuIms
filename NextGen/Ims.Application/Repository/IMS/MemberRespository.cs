using Enrich.IMS.Entities;
using Pos.Application.Extensions.Helper;
using Pos.Model.Model.Table;
using Pos.Model.Model.Table.IMS;
using Promotion.Application.Extensions;
using Promotion.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Repository.IMS
{
    public interface IMemberRespository : IEntityService<P_Member>
    {
        Task<P_Member> getMember(string MemberNumber);
        Task<List<LoadDepartment>> loadDepartment(string MemberNumber);
        Task<List<V_TicketType>> getTicketType(string BuildInCode);
        Task<List<P_Member>> getAllMember();
    }
    public class MemberRespository : IMSEntityService<P_Member>, IMemberRespository
    {
        public async Task<P_Member> getMember(string MemberNumber)
        {
            return await _connection.SqlFirstOrDefaultAsync<P_Member>(" select * from P_Member with (nolock) where MemberNumber=@MemberNumber", new { MemberNumber });
        }
        public async Task<List<P_Member>> getAllMember()
        {
            return (await _connection.SqlQueryAsync<P_Member>(" select * from P_Member with (nolock) ")).ToList();
        }

        public async Task<List<LoadDepartment>> loadDepartment(string MemberNumber)
        {
            return (await _connection.SqlQueryAsync<LoadDepartment>("exec P_GetListDepartment @MemberNumber", new { MemberNumber })).ToList();
        }

        public async Task<List<V_TicketType>> getTicketType(string BuildInCode)
        {
            return (await _connection.SqlQueryAsync<V_TicketType>("select * from V_TicketType where BuildInCode = @BuildInCode Or '0' = @BuildInCode", new { BuildInCode })).ToList();
        }
    }
}
