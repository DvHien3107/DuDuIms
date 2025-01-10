using Enrich.IMS.Dto.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Builders
{
    public interface ITicketBuilder
    {
        Task BuildForSave(bool isNew, TicketDto ticket);
    }
}
