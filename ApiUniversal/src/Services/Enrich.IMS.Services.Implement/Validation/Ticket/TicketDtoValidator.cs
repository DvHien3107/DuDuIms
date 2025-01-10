using Enrich.IMS.Dto.Ticket;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Validation
{
    public class TicketDtoValidator : BaseEnrichValidator<TicketDto>
    {
        public TicketDtoValidator()
        {
            RuleFor(a => a.Name).NotEmpty();
        }
    }
}
