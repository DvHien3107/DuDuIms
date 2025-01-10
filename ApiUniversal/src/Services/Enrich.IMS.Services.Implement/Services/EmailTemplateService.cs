using Enrich.IMS.Dto.EmailTemplate;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Services.Implement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public class EmailTemplateService : GenericService<EmailTemplate, EmailTemplateDto>, IEmailTemplateService
    {
        private readonly IEmailTemplateRepository _repository;

        public EmailTemplateService(IEmailTemplateRepository repository, IEmailTemplateMapper mapper)
            : base(repository, mapper)
        {
            _repository = repository;
        }
    }
}
