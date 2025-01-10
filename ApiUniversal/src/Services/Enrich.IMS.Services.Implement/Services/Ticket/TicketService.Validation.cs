using Enrich.IMS.Dto.Ticket;
using Enrich.IMS.Services.Implement.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class TicketService
    {
        private async Task ValidateSaveTicketAsync(bool isNew, TicketDto newDto, TicketDto oldDto = null)
        {
            //validate request
            var validate = await _container.Resolve<EnrichValidationHelper>().ValidateAsync<TicketDto, TicketDtoValidator>(newDto);
            if (!validate.IsValid)
            {
                ThrowValidations(validate.Errors);
            }
            
        }
    }
}
