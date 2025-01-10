using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Table.IMS
{
    public class License_Product_Item
    {
        public long Id { get; set; }

        public string License_Product_Id { get; set; }

        public int? CountWarning { get; set; }

        public string License_Item_Code { get; set; }

        public int? Value { get; set; }

        public bool? Enable { get; set; }

    }

}
