using Pos.Application.Repository.IMS;
using Pos.Model.Model.Table.IMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Services.Scoped.IMS
{
    public interface IMemberService
    {
        Task<P_Member> getMemberByNumber(string memberNumber);
        Task<List<LoadDepartment>> loadDepartment(string MemberNumber);
    }
    public class MemberService : IMemberService
    {
        private IMemberRespository _memberRespository;
        public MemberService(IMemberRespository memberRespository) {
            _memberRespository = memberRespository;
        }

        public async Task<P_Member> getMemberByNumber (string memberNumber)
        {
           return  await _memberRespository.getMember(memberNumber);
        }

        public async Task<List<LoadDepartment>> loadDepartment(string MemberNumber)
        {
            List<LoadDepartment> result = await _memberRespository.loadDepartment(MemberNumber);
            List<V_TicketType> lstTicketType = await _memberRespository.getTicketType("0");
            foreach (var item in result)
            {
                item.TicketTypes = lstTicketType.Where(x => x.BuildInCode == item.Id.ToString()).ToList();
            }
            return result;
        }
    }
}
