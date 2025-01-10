using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.DataTransfer.Twilio
{
    public class TollfreeVerificationRequest
    {
        public string BusinessStreetAddress { get; set; }
        public string BusinessStreetAddress2 { get; set; }
        public string BusinessCity { get; set; }
        public string BusinessStateProvinceRegion { get; set; }
        public string BusinessPostalCode { get; set; }
        public string BusinessCountry { get; set; }
        public string BusinessContactFirstName { get; set; }
        public string BusinessContactLastName { get; set; }
        public string BusinessContactEmail { get; set; }
        public string BusinessContactPhone { get; set; }
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
