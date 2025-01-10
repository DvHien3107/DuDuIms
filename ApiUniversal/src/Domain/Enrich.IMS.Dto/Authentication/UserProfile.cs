using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Authentication
{
    public class UserProfile
    {
        /// <summary>
        /// Gets or sets user identity.
        /// </summary>
        [JsonProperty("userId")]
        public long UserId { get; set; }


        /// <summary>
        /// Gets or sets PersonalEmail.
        /// </summary>    
        [JsonProperty("personalEmail")]
        public string PersonalEmail { get; set; }

    }
}
