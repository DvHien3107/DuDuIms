using Enrich.IMS.Dto;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Validation
{
    public class OrderValidator : BaseEnrichValidator<OrderDto>
    {
        public OrderValidator()
        {
            RuleFor(a => a.Id).NotNull();
        }
    }
}
