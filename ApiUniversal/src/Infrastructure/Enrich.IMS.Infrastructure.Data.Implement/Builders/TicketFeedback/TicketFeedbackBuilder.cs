using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Builders;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using System;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Builders
{
    public class TicketFeedbackBuilder : ITicketFeedbackBuilder
    {
        private readonly EnrichContext _context;
        private readonly ISalesLeadCommentRepository _repository;
        private readonly ISystemConfigurationRepository _repositorySystem;

        public TicketFeedbackBuilder(
            EnrichContext context,
            ISalesLeadCommentRepository repository,
            ISystemConfigurationRepository repositorySystem)
        {
            _context = context;
            _repository = repository;
            _repositorySystem = repositorySystem;
        }

        /// <summary>
        /// Builder data for create new Sales Lead
        /// </summary>
        /// <param name="salesLead">Sales lead data dto</param>
        public async Task BuildForSave(bool isNew, TicketFeedbackDto ticketFeedback)
        {
            var actionDate = TimeHelper.GetUTCNow();
            ticketFeedback.GlobalStatus = "publish";
            ticketFeedback.DateCode = actionDate.ToString(Constants.Format.Date_yyMMdd);
            if (isNew)
            {
                ticketFeedback.Id = long.Parse(actionDate.ToString(Constants.Format.Date_yyMMddhhmmssfff));

                if (!ticketFeedback.CreateAt.HasValue)
                    ticketFeedback.CreateAt = actionDate;
                if (string.IsNullOrEmpty(ticketFeedback.CreateByName))
                    ticketFeedback.CreateByName = _context.UserFullName;
                if (string.IsNullOrEmpty(ticketFeedback.CreateByNumber))
                    ticketFeedback.CreateByNumber = _context.UserNumber;
            }
            else
            {
                if (!ticketFeedback.UpdateAt.HasValue)
                    ticketFeedback.UpdateAt = actionDate;
                if (string.IsNullOrEmpty(ticketFeedback.UpdateBy))
                    ticketFeedback.UpdateBy = _context.UserNumber;
            }
        }
    }
}