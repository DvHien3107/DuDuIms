using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto.Ticket;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface ITicketTypeService : IGenericService<TicketType, TicketTypeDto>, ILookupDataService
    {
        Task<IEnumerable<TicketTypeDto>> GetTicketTypesByProjectIdAsync(string projectId);
    }
}
