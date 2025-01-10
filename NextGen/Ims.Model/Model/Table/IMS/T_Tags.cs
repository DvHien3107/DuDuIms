using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Table.IMS
{
    public class T_Tags
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Color { get; set; }

        public DateTime? UpdateAt { get; set; }

        public string UpdateBy { get; set; }

        public int? SiteId { get; set; }

    }
}
