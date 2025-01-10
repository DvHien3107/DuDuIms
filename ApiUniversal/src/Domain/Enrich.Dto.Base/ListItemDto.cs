using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base
{
    public class ListItemDto
    {
        public List<ListItemContextMenuDto> ContextMenuItems { get; set; }

        public int? TotalRows { get; set; }
    }

    public class ListItemContextMenuDto 
    {
        public object Data { get; set; }

        public string Action { get; set; }

        public string IconUrl { get; set; }
    }
}
