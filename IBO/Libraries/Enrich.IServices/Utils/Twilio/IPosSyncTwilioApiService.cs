using Enrich.DataTransfer.Twilio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IServices.Utils
{
    public interface IPosSyncTwilioApiService
    {
        Task PosUpdateTwilioPhoneNumerAsync(PosUpdateTwilioPhoneNumberRequest request);
    }
}
