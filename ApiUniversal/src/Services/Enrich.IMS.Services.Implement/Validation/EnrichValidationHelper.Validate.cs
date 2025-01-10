using Enrich.Dto.Base.Exceptions;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Validation
{
    public partial class EnrichValidationHelper
    {
        public async Task<EnrichValidationResult> ValidateAsync<TDto, TValidator>(TDto dto,bool throwError = false, bool translateMessage = true)
            where TValidator : BaseEnrichValidator<TDto>
        {
            if (!(_container.Resolve(typeof(TValidator)) is TValidator validator))
            {
                return new EnrichValidationResult();
            }

            return await ProcessValidateAsync(dto, validator,  throwError, translateMessage);
        }

        private async Task<EnrichValidationResult> ProcessValidateAsync<TDto>(TDto dto, BaseEnrichValidator<TDto> validator,  bool throwError = false, bool translateMessage = true)
        {
            var failures = new List<ValidationFailure>();

            var defaultErrors = await validator.ValidateAsync(dto);
            failures.AddRange(defaultErrors.Errors);  

            var EnrichResult = await ConvertToEnrichValidationResultAsync(failures, translateMessage);

            if (!EnrichResult.IsValid && throwError)
            {
                EnrichExceptionUtils.ThrowValidations(EnrichResult.Errors, "ValidationExceptions");
            }

            return EnrichResult;
        }      

        private async Task<EnrichValidationResult> ConvertToEnrichValidationResultAsync(IEnumerable<ValidationFailure> failures, bool translateMessage = true)
        {
            var result = new EnrichValidationResult();

            foreach (var error in failures)
            {
                var feRules = ResolveRule(error);
                if (result.Errors.Any(a => a.FullField == error.PropertyName && a.Rules == feRules))
                {
                    continue;
                }

                result.Errors.Add(new EnrichValidationFailure
                {
                    FullField = error.PropertyName,
                    ModelField = ResolveModelField(error),
                    Rules = feRules,
                    Scope = ResolveScope(error),
                    Message = error.ErrorMessage,
                    ExternalFailure = error
                });
            }          

            return result;
        }

        private string ResolveModelField(ValidationFailure error)
        {
            //if (error.FormattedMessagePlaceholderValues != null
            //        && error.FormattedMessagePlaceholderValues.TryGetValue("PropertyName", out var propName))
            //{
            //    return propName.GetString();
            //}

            var fullPropNames = error.PropertyName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            return fullPropNames.Last();
        }

        private string ResolveRule(ValidationFailure error)
        {
            var configRule = ConfigRules.FirstOrDefault(a => a.ErrorCodes.Contains(error.ErrorCode));

            return configRule?.Rule ?? error.ErrorCode;
        }

        private string ResolveScope(ValidationFailure error)
        {
            var scope = string.Empty;

            var fullPropNames = error.PropertyName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (fullPropNames.Count > 1)
            {
                fullPropNames.RemoveAt(fullPropNames.Count - 1);
                scope = fullPropNames.Last() ?? string.Empty;
            }

            return scope;
        }     
    }
}
