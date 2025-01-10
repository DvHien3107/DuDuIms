using Enrich.DataTransfer;
using Enrich.DataTransfer.JiraConnector;
using Enrich.IServices;
using Enrich.IServices.Utils.EnrichUniversal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Services.Utils.EnrichUniversal
{
    public class ConnectorJiraService : IConnectorJiraService
    {
        private readonly string _apiUrl;
        private readonly string _apiCreateJiraIssue;
        private readonly string _apiUpdateJiraIssue;
        private readonly string _apiGetJiraProject;
        private readonly string _apiGetJiraStatus;
        private readonly string _apiGetProjectMapping;
        private readonly string _apiGetJiraProjectById;
        private readonly string _apiDeleteJiraProjectMappingById;
        private readonly string _apiGetStatusMapping;
        private readonly string _apiCreateStatusMapping;
        private readonly string _apiJiraCommentUrl;
        private readonly string _secretKey;
        private readonly string _secretValue;
        private readonly ILogService _logger;
        private readonly EnrichContext _enrichContext;

        public ConnectorJiraService(EnrichContext enrichContext, ILogService logger)
        {
            _enrichContext = enrichContext;
            _logger = logger;
            _apiUrl = ConfigurationManager.AppSettings["JiraConnectorBaseUrl"];
            _apiCreateJiraIssue = ConfigurationManager.AppSettings["CreateJiraIssueUrl"];
            _apiUpdateJiraIssue = ConfigurationManager.AppSettings["UpdateJiraIssueUrl"];
            _apiGetJiraProject = ConfigurationManager.AppSettings["ApiGetJiraProject"];
            _apiGetJiraStatus = ConfigurationManager.AppSettings["ApiGetJiraStatus"];
            _apiGetProjectMapping = ConfigurationManager.AppSettings["ApiGetProjectMapping"];
            _apiGetJiraProjectById = ConfigurationManager.AppSettings["ApiGetJiraProjectById"];
            _apiDeleteJiraProjectMappingById = ConfigurationManager.AppSettings["ApiDeleteJiraProjectMappingById"];
            _apiGetStatusMapping = ConfigurationManager.AppSettings["ApiGetStatusMapping"];
            _apiCreateStatusMapping = ConfigurationManager.AppSettings["ApiCreateStatusMapping"];
            _apiJiraCommentUrl = ConfigurationManager.AppSettings["JiraCommentUrl"];

        }

        public async Task CreateJiraIssue(long ticketId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_enrichContext.AccessToken}");

                    var data = new StringContent(JsonConvert.SerializeObject(new {ticketId = ticketId }));
                    data.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    using (var response = await httpClient.PostAsync($"{_apiUrl}/{_apiCreateJiraIssue}", data))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new Exception(responseJson);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[CreateJiraIssue] create jira issue error, ticket id {ticketId}");
            }
        }

        public async Task UpdateJiraIssue(long ticketId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_enrichContext.AccessToken}");

                    var data = new StringContent(JsonConvert.SerializeObject(new { ticketId = ticketId }));
                    data.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    using (var response = await httpClient.PostAsync($"{_apiUrl}/{_apiUpdateJiraIssue}", data))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new Exception(responseJson);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[CreateJiraIssue] create jira issue error, ticket id {ticketId}");
            }
        }


        public async Task<List<JiraProjectDto>> GetJiraProjects()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_enrichContext.AccessToken}");
                    using (var response = await httpClient.GetAsync($"{_apiUrl}/{_apiGetJiraProject}"))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseJson);
                        }
                        string contentResponse = await response.Content.ReadAsStringAsync();
                        var reponse = JsonConvert.DeserializeObject<List<JiraProjectDto>>(contentResponse);
                        return reponse;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[FetchNewCustomerGoal] get merchant error");
                throw ex;
            }
        }
        public async Task<JiraProjectDto> GetJiraProjectById(string projectId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_enrichContext.AccessToken}");
                    using (var response = await httpClient.GetAsync($"{_apiUrl}/{string.Format(_apiGetJiraProjectById,projectId)}"))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseJson);
                        }
                        string contentResponse = await response.Content.ReadAsStringAsync();
                        var reponse = JsonConvert.DeserializeObject<JiraProjectDto>(contentResponse);
                        return reponse;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[FetchNewCustomerGoal] get merchant error");
                throw ex;
            }
        }

        public async Task<List<JiraStatusDto>> GetJiraStatusByProjectId(string projectId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_enrichContext.AccessToken}");
                    using (var response = await httpClient.GetAsync($"{_apiUrl}/{string.Format(_apiGetJiraStatus,projectId)}"))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseJson);
                        }
                        string contentResponse = await response.Content.ReadAsStringAsync();
                        var reponse = JsonConvert.DeserializeObject<List<JiraStatusDto>>(contentResponse);
                        return reponse;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[FetchNewCustomerGoal] get merchant error");
                throw ex;
            }
        }

        public async Task<List<ProjectMappingDto>> GetAllJiraProjectsMapping()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_enrichContext.AccessToken}");
                    using (var response = await httpClient.GetAsync($"{_apiUrl}/{_apiGetProjectMapping}"))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseJson);
                        }
                        string contentResponse = await response.Content.ReadAsStringAsync();
                        var reponse = JsonConvert.DeserializeObject<List<ProjectMappingDto>>(contentResponse);
                        return reponse;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[FetchNewCustomerGoal] get merchant error");
                throw ex;
            }
        }


        public async Task<ProjectMappingDto> GetJiraProjectsMappingById(string projectMappingId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_enrichContext.AccessToken}");
                    using (var response = await httpClient.GetAsync($"{_apiUrl}/{_apiGetProjectMapping}/{projectMappingId}"))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseJson);
                        }
                        string contentResponse = await response.Content.ReadAsStringAsync();
                        var reponse = JsonConvert.DeserializeObject<ProjectMappingDto>(contentResponse);
                        return reponse;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[FetchNewCustomerGoal] get merchant error");
                throw ex;
            }
        }

        public async Task InsertJiraProjectsMapping(ProjectMappingDto projectMappingDto)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_enrichContext.AccessToken}");
                    var data = new StringContent(JsonConvert.SerializeObject(projectMappingDto));
                    data.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    using (var response = await httpClient.PostAsync($"{_apiUrl}/{_apiGetProjectMapping}", data))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseJson);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[FetchNewCustomerGoal] get merchant error");
                throw ex;
            }
        }

        public async Task DeleteJiraProjectMapping(string jiraProjectMappingId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_enrichContext.AccessToken}");
                    using (var response = await httpClient.DeleteAsync($"{_apiUrl}/{string.Format(_apiDeleteJiraProjectMappingById, jiraProjectMappingId)}"))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.NoContent)
                        {
                            responseJson = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseJson);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[FetchNewCustomerGoal] get merchant error");
                throw ex;
            }
        }

        public async Task<List<StatusMappingDto>> GetStatusMappingByProjectId(string jiraProjectMappingId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_enrichContext.AccessToken}");
                    using (var response = await httpClient.GetAsync($"{_apiUrl}/{string.Format(_apiGetStatusMapping, jiraProjectMappingId)}"))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseJson);
                        }
                        string contentResponse = await response.Content.ReadAsStringAsync();
                        var reponse = JsonConvert.DeserializeObject<List<StatusMappingDto>>(contentResponse);
                        return reponse;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[FetchNewCustomerGoal] get merchant error");
                throw ex;
            }
        }

        public async Task SaveStatusMappingByProjectId(string jiraProjectMappingId, List<StatusMappingDto> statusMappingDtos)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_enrichContext.AccessToken}");
                    var data = new StringContent(JsonConvert.SerializeObject(statusMappingDtos));
                    data.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    using (var response = await httpClient.PostAsync($"{_apiUrl}/{string.Format(_apiCreateStatusMapping, jiraProjectMappingId)}", data))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseJson);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[FetchNewCustomerGoal] get merchant error");
                throw ex;
            }
        }

        public async Task CreateJiraComment(long commentId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_enrichContext.AccessToken}");

                    var data = new StringContent(JsonConvert.SerializeObject(new { }));
                    data.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    using (var response = await httpClient.PostAsync($"{_apiUrl}/{_apiJiraCommentUrl}/create/{commentId}", data))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.Created)
                        {
                            responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new Exception(responseJson);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[CreateJiraComment] create jira comment error, comment id {commentId}");
            }
        }
        public async Task UpdateJiraComment(long commentId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_enrichContext.AccessToken}");

                    var data = new StringContent(JsonConvert.SerializeObject(new { }));
                    data.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    using (var response = await httpClient.PostAsync($"{_apiUrl}/{_apiJiraCommentUrl}/update/{commentId}", data))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new Exception(responseJson);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[UpdateJiraComment] update jira comment error, comment id {commentId}");
            }
        }
    }
}
