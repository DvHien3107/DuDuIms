using Enrich.IMS.Dto;
using FluentValidation;

namespace Enrich.IMS.Services.Implement.Validation
{
    public class SalesLeadDtoValidator : BaseEnrichValidator<SalesLeadDto>
    {
        public SalesLeadDtoValidator()
        {
            RuleFor(a => a.ContactName).NotEmpty();
            RuleFor(a => a.SalonName).NotEmpty();
            RuleFor(a => a.SalonEmail).NotEmpty().EmailAddress();
            RuleFor(a => a.Country).NotEmpty();
            RuleFor(a => a.SalonTimeZone).NotEmpty();
            RuleFor(a => a.SalonTimeZoneNumber).NotEmpty();
        }
    }
}
