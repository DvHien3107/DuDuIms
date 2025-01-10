using Enrich.DataTransfer.JiraConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IServices.Utils.EnrichUniversal
{
    public interface IConnectorJiraService
    {
        Task CreateJiraIssue(long ticketId);
        Task UpdateJiraIssue(long ticketId);
        Task<List<JiraProjectDto>> GetJiraProjects();
        Task<JiraProjectDto> GetJiraProjectById(string projectId);
        Task<List<JiraStatusDto>> GetJiraStatusByProjectId(string projectId);
        Task<List<ProjectMappingDto>> GetAllJiraProjectsMapping();
        Task<ProjectMappingDto> GetJiraProjectsMappingById(string projectMappingId);
        Task InsertJiraProjectsMapping(ProjectMappingDto projectMappingDto);
        Task DeleteJiraProjectMapping(string jiraProjectMapping);
        Task<List<StatusMappingDto>> GetStatusMappingByProjectId(string jiraProjectMappingId);
        Task SaveStatusMappingByProjectId(string jiraProjectMappingId,List<StatusMappingDto> statusMappingDtos);

        Task CreateJiraComment(long commentId);
        Task UpdateJiraComment(long commentId);
    }
}
