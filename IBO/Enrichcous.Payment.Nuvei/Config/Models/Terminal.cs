using System.Xml.Serialization;

namespace Enrichcous.Payment.Nuvei.Config.Models
{
    public class Terminal
    {
        [XmlAttribute("name")] public string Name { get; set; }
        [XmlAttribute("id")] public string Id { get; set; }
        [XmlAttribute("currency")] public string Currency { get; set; }
        [XmlAttribute("secret")] public string Secret { get; set; }
        
        // Custom
        public string Gateway { get; set; } = "nuvei";

        public bool IsMultiCur()
        {
            return Currency == "MCP";
        }
    }
}