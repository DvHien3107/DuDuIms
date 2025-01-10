using Enrich.IMS.Dto;
using FluentValidation;

namespace Enrich.IMS.Services.Implement.Validation
{
    public class TicketFeedbackValidator : BaseEnrichValidator<TicketFeedbackDto>
    {
        public TicketFeedbackValidator()
        {
            RuleFor(a => a.TicketId).NotEmpty();
        }
    }
}
