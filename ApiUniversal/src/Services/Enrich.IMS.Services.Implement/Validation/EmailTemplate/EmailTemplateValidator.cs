using Enrich.IMS.Dto.EmailTemplate;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Validation
{
    public class EmailTemplateValidator : BaseEnrichValidator<EmailTemplateDto>
    {
        public EmailTemplateValidator()
        {
            RuleFor(a => a.Name).NotEmpty().MaximumLength(255);
        }
    }
}
