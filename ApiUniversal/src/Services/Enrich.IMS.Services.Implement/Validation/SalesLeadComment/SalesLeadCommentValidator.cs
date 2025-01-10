using Enrich.IMS.Dto;
using FluentValidation;

namespace Enrich.IMS.Services.Implement.Validation
{
    public class SalesLeadCommentValidator : BaseEnrichValidator<SalesLeadCommentDto>
    {
        public SalesLeadCommentValidator()
        {
            RuleFor(a => a.Title).NotEmpty();
            RuleFor(a => a.Description).NotEmpty();
            RuleFor(a => a.SalesLeadId).NotEmpty();
        }
    }
}
