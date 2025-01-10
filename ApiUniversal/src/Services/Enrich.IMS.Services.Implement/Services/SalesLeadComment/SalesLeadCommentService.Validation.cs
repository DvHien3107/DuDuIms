
using Enrich.Common.Enums;
using Enrich.Dto.Base.Exceptions;
using Enrich.IMS.Dto;
using Enrich.IMS.Services.Implement.Validation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class SalesLeadCommentService
    {
        private async Task ValidateSaveAsync(bool isNew, SalesLeadCommentDto newDto)
        {
            //validate request
            var validate = await _container.Resolve<EnrichValidationHelper>().ValidateAsync<SalesLeadCommentDto, SalesLeadCommentValidator>(newDto);
            if (!validate.IsValid)
            {
                ThrowValidations(validate.Errors);
            }

            //validate salesLeadId is exist
            var salesLead = await _salesLeadRepository.FindByIdAsync(newDto.SalesLeadId);
            if (salesLead == null)
            {
                ThrowValidations(new List<EnrichValidationFailure>() {
                    new EnrichValidationFailure()
                    {
                        Message = ValidationMessages.NotFound.SalesLead,
                        ModelField = nameof(SalesLeadCommentDto.SalesLeadId),
                    }
                });
            }
        }
    }
}