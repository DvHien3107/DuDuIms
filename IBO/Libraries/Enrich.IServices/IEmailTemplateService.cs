using Enrich.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IServices
{
    public interface IEmailTemplateService
    {
        List<EmailTemplate> GetAllEmailTemplate();
    }
}