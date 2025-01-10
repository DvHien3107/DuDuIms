using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Enrich.RestApi.NetCorePlatform.Errors
{
    public enum ServiceErrorEnum
    {
        [XmlEnum("0")]
        GeneralError = 0,

        [XmlEnum("1")]
        ValidationError = 1,

        [XmlEnum("10")]
        NotFound = 10,

        [XmlEnum("20")]
        InternalServerError = 20,

        [XmlEnum("30")]
        Unauthorized = 30,

        [XmlEnum("31")]
        InvalidToken = 31,
    }
}
