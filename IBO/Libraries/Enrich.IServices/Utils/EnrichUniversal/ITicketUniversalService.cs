using Enrich.DataTransfer.EnrichUniversal.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IServices.Utils.EnrichUniversal
{
    public interface ITicketUniversalService
    {
        Task<List<ProjectMilestoneDto>> GetAllTicketProject();
        Task<ProjectMilestoneDto> GetProjectById(string projectId);
        Task<List<TicketStatusDto>> GetTicketStatusByProjectId(string projectId);
        Task<List<TicketTypeDto>> GetTicketTypesByProjectId(string projectId);
    }
}
