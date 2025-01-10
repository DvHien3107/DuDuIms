using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Enrich.Payment.MxMerchant.Utils
{
    public class OrderedContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(System.Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization).OrderBy(p => p.PropertyName).ToList();
        }
    }
}
