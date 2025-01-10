using Enrich.DataTransfer.JiraConnector;
using Enrich.IServices.Utils.EnrichUniversal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    public class JiraConfigurationMappingController : Controller
    {
        private readonly ITicketUniversalService _ticketService;
        private readonly IConnectorJiraService _connectorJiraService;

        public JiraConfigurationMappingController(ITicketUniversalService ticketService, IConnectorJiraService connectorJiraIssueService)
        {
            _ticketService = ticketService;
            _connectorJiraService = connectorJiraIssueService;
        }

        #region jira mapping configuration
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> LoadJiraConfigurationMapping()
        {
            var list = await _connectorJiraService.GetAllJiraProjectsMapping();
            return Json(new
            {
                recordsFiltered = list.Count,
                recordsTotal = list.Count,
                data = list,
            });
        }

        [HttpGet]
        public async Task<ActionResult> CreateOrUpdateProjectMapping()
        {
            var imsProjects = await _ticketService.GetAllTicketProject();
            var jiraProjects = await _connectorJiraService.GetJiraProjects();
            ViewBag.IMSProjects = imsProjects;
            ViewBag.JiraProjects = jiraProjects;
            return PartialView("_CreateJiraProjectMappingPopup");
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrUpdateProjectMapping(ProjectMappingDto model)
        {
            try
            {
                var imsProject = await _ticketService.GetProjectById(model.IMSId);
                model.IMSName = imsProject.Name;

                var jiraProject = await _connectorJiraService.GetJiraProjectById(model.JiraId);
                model.JiraName = jiraProject.Name;

                await _connectorJiraService.InsertJiraProjectsMapping(model);
                
                return Json(new { status = true, message = "create project mapping success" });
            }
            catch(Exception ex)
            {
                return Json(new { status = false, message = $"create project mapping failed: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<ActionResult> CreateOrUpdateConfigMapping(string projectMappingId)
        {
            var projectMapping = await _connectorJiraService.GetJiraProjectsMappingById(projectMappingId);
            var imsTicketStatus = await _ticketService.GetTicketStatusByProjectId(projectMapping.IMSId);
            var jiraTicketStatus = await _connectorJiraService.GetJiraStatusByProjectId(projectMapping.JiraId);
            var statusMappings = await _connectorJiraService.GetStatusMappingByProjectId(projectMappingId);

            ViewBag.IMSStatus = imsTicketStatus.OrderByDescending(x=>x.Type=="open").ThenBy(x=>x.Name).ToList();
            ViewBag.JiraStatus = jiraTicketStatus;
            ViewBag.StatusMappings = statusMappings;

            ViewBag.ProjectMappingId = projectMappingId;
            return PartialView("_JiraConfigurationFieldsMappingPopup");
        }
        [HttpPost]
        public async Task<ActionResult> CreateOrUpdateConfigMapping(string projectMappingId,List<StatusMappingDto> jiraStatusDtos)
        {
            try
            {
                 jiraStatusDtos = jiraStatusDtos.Where(x => !string.IsNullOrEmpty(x.JiraId)).ToList();
                 await _connectorJiraService.SaveStatusMappingByProjectId(projectMappingId,jiraStatusDtos);
                return Json(new { status = true, message = "update project mapping field success" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = $"create project mapping field failed: {ex.Message}" });
            }         
        }

        [HttpPost]
        public async Task<ActionResult> DeleteProjectMapping(string projectMappingId)
        {
            try
            {
                await _connectorJiraService.DeleteJiraProjectMapping(projectMappingId);
                return Json(new { status = true, message = "delete project mapping success" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = $"delete project mapping failed: {ex.Message}" });
            }
        }
        #endregion
    }
}