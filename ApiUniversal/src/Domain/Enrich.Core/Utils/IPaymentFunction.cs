using Enrich.Dto.Base;
using Enrich.IMS.Dto.Sms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;

namespace Enrich.Core.Utils
{
    public interface IPaymentFunction
    {
        public MxMerchantResponseDto MakePayment(MxMerchantDto dataRequest);
        public MxMerchantResponseDto MakePaymentACH(MxMerchantDto dataRequest);
    }
}
