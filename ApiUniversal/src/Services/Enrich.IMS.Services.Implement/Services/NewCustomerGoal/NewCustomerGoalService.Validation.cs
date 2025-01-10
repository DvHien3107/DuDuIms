using Enrich.Core.Infrastructure.Repository;
using Enrich.Dto.Base.Exceptions;
using Enrich.IMS.Dto;
using Enrich.IMS.Services.Implement.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class NewCustomerGoalService
    {
        private async Task ValidateSaveAsync(bool isNew, NewCustomerGoalDto newDto, NewCustomerGoalDto oldDto = null)
        {
            //validate request
            var validate = await _container.Resolve<EnrichValidationHelper>().ValidateAsync<NewCustomerGoalDto, NewCustomerGoalValidator>(newDto);
            if (!validate.IsValid)
            {
                ThrowValidations(validate.Errors);
            }
            if (isNew || 
                (newDto.Year != oldDto.Year || newDto.Month != oldDto.Month))
            {
                var conditionExist = await _repository.IsExistGoalForTime(newDto.Year, newDto.Month);
                if (conditionExist)
                    ThrowValidations(new List<EnrichValidationFailure>()
                {
                    new EnrichValidationFailure()
                    {
                        Message = "Goal for time is exist in DataBase",
                        //ModelField = nameof(NewCustomerGoalDto.SalonEmail),
                    }
                });
            }
        }
    }
}
