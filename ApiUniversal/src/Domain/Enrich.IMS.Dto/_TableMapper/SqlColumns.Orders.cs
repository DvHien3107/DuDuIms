using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto
{
    public sealed partial class SqlColumns
    {
        public sealed class Orders
        {
            public const string NoteDelivery = "Note_Delivery";
            public const string NotePackaging = "Note_Packaging";
            public const string ServiceAmount = "Service_Amount";
            public const string TotalHardwareAmount = "TotalHardware_Amount";
        }
    }
}
