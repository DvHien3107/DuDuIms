using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace Enrich.Identity
{
    public class EnrichIdentity : IIdentity
    {
        [JsonIgnore]
        public string AuthenticationType => "Bearer";

        [JsonIgnore]
        public bool IsAuthenticated => true;


        [JsonProperty("userId")]
        public long UserId { get; set; }

        [JsonProperty("personalEmail")]
        public string PersonalEmail { get; set; }

        public string Name { get; }
        public EnrichIdentity()
        {
        }

    }
}
