using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.DataTransfer.Twilio
{
   public class TollfreeVerificationResponse
    {
        public string sid { get; set; }
        public string account_sid { get; set; }
        public string regulated_item_sid { get; set; }
        public string customer_profile_sid { get; set; }
        public string trust_product_sid { get; set; }
        public string status { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_updated { get; set; }
        public string business_name { get; set; }
        public string business_street_address { get; set; }
        public string business_street_address2 { get; set; }
        public string business_city { get; set; }
        public string business_state_province_region { get; set; }
        public string business_postal_code { get; set; }
        public string business_country { get; set; }
        public string business_website { get; set; }
        public string business_contact_first_name { get; set; }
        public string business_contact_last_name { get; set; }
        public string business_contact_email { get; set; }
        public string business_contact_phone { get; set; }
        public string notification_email { get; set; }
        public List<string> use_case_categories { get; set; }
        public string use_case_summary { get; set; }
        public string production_message_sample { get; set; }
        public List<string> opt_in_image_urls { get; set; }
        public string opt_in_type { get; set; }
        public string message_volume { get; set; }
        public string additional_information { get; set; }
        public string tollfree_phone_number_sid { get; set; }
        public string rejection_reason { get; set; }
        public string error_code { get; set; }
        public string edit_expiration { get; set; }
        public Dictionary<string, string> resource_links { get; set; }
        public string url { get; set; }
        public string external_reference_id { get; set; }
    }
}
