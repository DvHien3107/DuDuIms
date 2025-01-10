using Enrich.Dto.Requests;

namespace Enrich.IMS.Dto.TicketFeedback
{
    public class TicketFeedbackUpdateRequest : BaseSaveByJsonPatchRequest<TicketFeedbackDto>
    {
        public TicketFeedbackUpdateOption UpdateOption { get; set; }
        public TicketFeedbackUpdateRequest(TicketFeedbackDto dto, TicketFeedbackDto oldDto = null) : this()
        {
            NewDto = dto;
            OldDto = oldDto;
        }

        public TicketFeedbackUpdateRequest()
        {
        }
    }
}
