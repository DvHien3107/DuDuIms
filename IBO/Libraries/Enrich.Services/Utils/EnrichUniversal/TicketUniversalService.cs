using Enrich.DataTransfer;
using Enrich.DataTransfer.EnrichUniversal.Ticket;
using Enrich.IServices;
using Enrich.IServices.Utils.EnrichUniversal;
using Enrich.IServices.Utils.Mailing;
using Enrich.IServices.Utils.SMS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Services.Utils.EnrichUniversal
{
    public class TicketUniversalService : ITicketUniversalService
    {
        private readonly string _apiUrl;
        private readonly string _secretKey;
        private readonly string _apiGetTicketProject;
        private readonly string _apiGetTicketStatus;
        private readonly string _apiGetTicketType;
        private readonly string _apiGetTicketProjectById;
        private readonly ILogService _logger;
        private readonly EnrichContext _enrichContext;
        private readonly IEnrichEmailService _enrichEmailService;
        private readonly IEnrichSMSService _enrichSMSService;

        public TicketUniversalService(
            ILogService logger,
            EnrichContext enrichContext,
            IEnrichEmailService enrichEmailService,
            IEnrichSMSService enrichSMSService)
        {
            _apiUrl = ConfigurationManager.AppSettings["ApiUniversalUrl"];
            _secretKey = ConfigurationManager.AppSettings["UniversalBasicKey"];
            _apiGetTicketProject = ConfigurationManager.AppSettings["ApiGetTicketProject"];
            _apiGetTicketStatus = ConfigurationManager.AppSettings["ApiGetTicketStatus"];
            _apiGetTicketType = ConfigurationManager.AppSettings["ApiGetTicketType"];
            _apiGetTicketProjectById = ConfigurationManager.AppSettings["ApiGetTicketProjectById"];
            _logger = logger;
            _enrichContext = enrichContext;
            _enrichEmailService = enrichEmailService;
            _enrichSMSService = enrichSMSService;
        }

        public async Task<List<ProjectMilestoneDto>> GetAllTicketProject()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");
                    using (var response = await httpClient.GetAsync($"{_apiUrl}/{_apiGetTicketProject}"))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseJson);
                        }
                        string contentResponse = await response.Content.ReadAsStringAsync();
                        var reponse = JsonConvert.DeserializeObject<List<ProjectMilestoneDto>>(contentResponse);
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

        public async Task<ProjectMilestoneDto> GetProjectById(string projectId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");
                    using (var response = await httpClient.GetAsync($"{_apiUrl}/{string.Format(_apiGetTicketProjectById, projectId)}"))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseJson);
                        }
                        string contentResponse = await response.Content.ReadAsStringAsync();
                        var reponse = JsonConvert.DeserializeObject<ProjectMilestoneDto>(contentResponse);
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

        public async Task<List<TicketStatusDto>> GetTicketStatusByProjectId(string projectId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");
                    using (var response = await httpClient.GetAsync($"{_apiUrl}/{string.Format(_apiGetTicketStatus, projectId)}"))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseJson);
                        }
                        string contentResponse = await response.Content.ReadAsStringAsync();
                        var reponse = JsonConvert.DeserializeObject<List<TicketStatusDto>>(contentResponse);
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

        public async Task<List<TicketTypeDto>> GetTicketTypesByProjectId(string projectId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");
                    using (var response = await httpClient.GetAsync($"{_apiUrl}/{string.Format(_apiGetTicketType, projectId)}"))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseJson);
                        }
                        string contentResponse = await response.Content.ReadAsStringAsync();
                        var reponse = JsonConvert.DeserializeObject<List<TicketTypeDto>>(contentResponse);
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
    }
}
