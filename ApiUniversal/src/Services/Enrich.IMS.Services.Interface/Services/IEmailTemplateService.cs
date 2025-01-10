using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto.EmailTemplate;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface IEmailTemplateService : IGenericService<EmailTemplate, EmailTemplateDto>
    {
    }
}
