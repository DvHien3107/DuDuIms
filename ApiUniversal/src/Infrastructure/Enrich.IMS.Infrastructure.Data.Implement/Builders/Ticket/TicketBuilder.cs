using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Ticket;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Builders;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Builders
{
    public class TicketBuilder : ITicketBuilder
    {
        private readonly EnrichContext _context;
        private readonly ITicketRepository _repository;
        private readonly ISystemConfigurationRepository _repositorySystem;

        public TicketBuilder(
            EnrichContext context,
            ITicketRepository repository,
            ISystemConfigurationRepository repositorySystem)
        {
            _context = context;
            _repository = repository;
            _repositorySystem = repositorySystem;
        }

        /// <summary>
        /// Builder data for create new Sales Lead
        /// </summary>
        /// <param name="ticket">Sales lead data dto</param>
        public async Task BuildForSave(bool isNew, TicketDto ticket)
        {
            ticket.TypeId = string.Join(",", ticket.Types?.Select(x => x.Id));
            ticket.TypeName = string.Join(", ", ticket.Types?.Select(x => x.TypeName));
            //ticket.StatusId = ticket.Status?.Id.ToString();
            //ticket.StatusName = ticket.Status?.Name;
            ticket.UpdateAt = TimeHelper.GetUTCNow();
            if (string.IsNullOrEmpty(ticket.UpdateBy))
                ticket.UpdateBy = _context.UserFullName;
        }
    }
}