using System.Collections.Generic;
using System.Xml.Serialization;

namespace Enrichcous.Payment.Nuvei.Config.Models
{
    [XmlRoot("root")]
    public class AppData {
        [XmlArray ("terminals"), XmlArrayItem("terminal")]
        public List<Terminal> Terminals {get;set;}
        [XmlElement("account_test")]
        public bool? AccountTest { get; set; }
    }
}