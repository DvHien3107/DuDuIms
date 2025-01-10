using Enrich.IMS.Dto.SendEmail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Core.Utils
{
    public interface IEnrichSendEmail
    {

        Task Send(SendEmailRequest request);
        Task SendBySendGridTemplateId(SendEmailBySendGridTemplateId request);
    }
}
