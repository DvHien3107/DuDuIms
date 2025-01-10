using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Common
{
    /// <summary>
    /// filter by traditional search
    /// </summary>
    public class BaseSearchCondition
    {
        public int[] Ids { get; set; }

        public int[] ExcludeIds { get; set; }

        public bool IsDeleted { get; set; }
    }
}
