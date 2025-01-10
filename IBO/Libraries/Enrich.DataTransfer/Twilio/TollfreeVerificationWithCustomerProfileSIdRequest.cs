using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.DataTransfer.Twilio
{
    public class TollfreeVerificationWithCustomerProfileSIdRequest
    {
        public string CustomerProfileSid { get; set; }
        public string AdditionalInformation { get; set; }
        public string ExternalReferenceId { get; set; }
        public string BusinessName { get; set; }
        public string BusinessWebsite { get; set; }
        public string NotificationEmail { get; set; }
        public List<string> UseCaseCategories { get; set; }
        public string UseCaseSummary { get; set; }
        public string ProductionMessageSample { get; set; }
        public List<string> OptInImageUrls { get; set; }
        public string OptInType { get; set; }
        public string MessageVolume { get; set; }
        public string TollfreePhoneNumberSid { get; set; }
    }
}
