
using Enrich.Common.Enums;
using Enrich.Dto.Base.Exceptions;
using Enrich.IMS.Dto;
using Enrich.IMS.Services.Implement.Validation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class TicketFeedbackService
    {
        private async Task ValidateSaveAsync(bool isNew, TicketFeedbackDto newDto)
        {
            //validate request
            var validate = await _container.Resolve<EnrichValidationHelper>().ValidateAsync<TicketFeedbackDto, TicketFeedbackValidator>(newDto);
            if (!validate.IsValid)
            {
                ThrowValidations(validate.Errors);
            }

            //validate salesLeadId is exist
            var ticket = await _ticketRepository.FindByIdAsync(newDto.TicketId);
            if (ticket == null)
            {
                ThrowValidations(new List<EnrichValidationFailure>() {
                    new EnrichValidationFailure()
                    {
                        Message = ValidationMessages.NotFound.Ticket,
                        ModelField = nameof(TicketFeedbackDto.TicketId),
                    }
                });
            }
        }
    }
}