using Enrich.IMS.Dto;
using FluentValidation;

namespace Enrich.IMS.Services.Implement.Validation
{
    public class NewCustomerGoalValidator : BaseEnrichValidator<NewCustomerGoalDto>
    {
        public NewCustomerGoalValidator()
        {
            RuleFor(a => a.Year).NotEmpty().GreaterThan(2018).LessThan(3000);
            RuleFor(a => a.Month).NotEmpty().GreaterThanOrEqualTo(1).LessThanOrEqualTo(12);
            RuleFor(a => a.Month).NotEmpty().GreaterThan(0);
        }
    }
}
