
using Enrich.Dto.Base.Exceptions;
using Enrich.IMS.Dto;
using Enrich.IMS.Services.Implement.Validation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class SalesLeadService
    {
        private async Task ValidateSaveSalesLeadAsync(bool isNew, SalesLeadDto newDto, SalesLeadDto oldDto = null)
        {
            //validate request
            var validate = await _container.Resolve<EnrichValidationHelper>().ValidateAsync<SalesLeadDto, SalesLeadDtoValidator>(newDto);
            if (!validate.IsValid)
            {
                ThrowValidations(validate.Errors);
            }
            if (isNew || newDto.SalonEmail != oldDto.SalonEmail)
            {
                var conditionExistEmail = _repository.IsExistEmail(newDto.SalonEmail) || _repositoryCustomer.IsExistEmail(newDto.SalonEmail);
                if (conditionExistEmail)
                    ThrowValidations(new List<EnrichValidationFailure>()
                {
                    new EnrichValidationFailure()
                    {
                        Message = "Email is exist in DataBase",
                        ModelField = nameof(SalesLeadDto.SalonEmail),
                    }
                });
            }
        }
    }
}