using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.SalesLeadComment;
using Enrich.IMS.Dto.TicketFeedback;
using Enrich.IMS.Entities;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface ITicketFeedbackService : IGenericService<TicketFeedback, TicketFeedbackDto>
    {
        Task<TicketFeedbackDto> GetByIdAsync(long id);

        Task<TicketFeedbackUpdateResponse> CreateAsync(TicketFeedbackDto dto);

        Task<TicketFeedbackUpdateResponse> UpdateAsync(TicketFeedbackUpdateRequest request);
    }
}