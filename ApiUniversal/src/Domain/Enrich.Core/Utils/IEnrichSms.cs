using Enrich.IMS.Dto.Sms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;

namespace Enrich.Core.Utils
{
    public interface IEnrichSms
    {
        Task<MessageResource> SendAsync(SendSmsRequest request);

    }
}
