using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Ticket;
using Enrich.IMS.Entities;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.Services.Implement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Mappers
{
    public class ProjectMilestoneMapper : BaseMapperAutoMapper<ProjectMilestone, ProjectMilestoneDto>, IProjectMilestoneMapper
    {
        public ProjectMilestoneMapper() : base()
        {
        }
    }
}
