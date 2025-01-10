using Enrich.Common.Enums;
using Enrich.Dto.Attributes;
using Enrich.Dto.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Ticket
{
    public partial class TicketListItemDto : ListItemDto
    {      

        [GridField(Index = 1, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto("T_SupportTicket.ID")]
        public int Id { get; set; }


        [GridField(Index = 2, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto("T_SupportTicket.Name")]
        public string Name { get; set; }

        /// <summary>
        /// Populate after called search
        /// </summary>
        [GridField(Index = 2, IsDefault = true, IsShow = true, DisplayType = ColDisplayType.Icon)]
        public string TicketStatus { get; set; }   
    }
}
