using Enrich.IMS.Entities;
using Pos.Application.Repository.IMS;
using Pos.Model.Model.Table.IMS;
using Pos.Model.Model.Table.IMS.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Services.Scoped.IMS
{
    public interface ITicketService
    {
        Task<List<LoadTicket>> LoadTicket(string DepartmentID, string TypeId, DateTime start, DateTime end);
    }
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepo;
        private readonly IMemberRespository _memberRepo;

        public TicketService(ITicketRepository ticketRepo, IMemberRespository memberRepo)
        {
            _ticketRepo = ticketRepo;
            _memberRepo = memberRepo;
        }

        public async Task<List<LoadTicket>> LoadTicket(string DepartmentID, string TypeId, DateTime start, DateTime end)
        {
            List<LoadTicket> result = (await _ticketRepo.LoadTicket(DepartmentID, TypeId, start, end));
            List<P_Member> lstMem = await _memberRepo.getAllMember();
            List<T_Tags> lstTags = await _ticketRepo.getAllTags();
            foreach (var item in result)
            {
                if (item.TagMemberNumber != null)
                {
                    item.lstMember = lstMem.Where(x => item.TagMemberNumber.Contains(x.MemberNumber)).ToList();
                }
                if (item.AssignedToMemberNumber != null)
                {
                    item.lstAssigned = lstMem.Where(x => item.AssignedToMemberNumber.Contains(x.MemberNumber)).ToList();
                }
                if (item.Tags != null)
                {
                    item.lstTags = lstTags.Where(x => item.Tags.Contains(x.Id)).ToList();
                }
            }
            return result;
        }
    }
}
