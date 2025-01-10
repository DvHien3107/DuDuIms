using Enrich.Common;
using Enrich.Core.Configs;
using Enrich.Core.Utils;
using Enrich.Dto.Base;
using Enrich.IMS.Dto.SendEmail;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Enrich.Infrastructure.Email
{
    public partial class EnrichSendEmail : IEnrichSendEmail
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAppConfig _appConfig;
        public EnrichSendEmail(IHttpClientFactory httpClientFactory, IAppConfig appConfig)
        {
            _httpClientFactory = httpClientFactory;
            _appConfig = appConfig;
        }

        public async Task Send(SendEmailRequest request)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_appConfig.IMSEmailBaseServiceUrl);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_appConfig.IMSEmailServiceClientSecret}");
            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            using var response = await httpClient.PostAsync(_appConfig.IMSEmailSendUrl, stringContent).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            else
            {
                var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var payload = JsonConvert.DeserializeObject<Response>(payloadString);

                if (payload.Code == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException(payload.Message);

                throw new HttpResponseException(payload.Code, payload.Message);
            }
        }
        public async Task SendBySendGridTemplateId(SendEmailBySendGridTemplateId request)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_appConfig.IMSEmailBaseServiceUrl);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_appConfig.IMSEmailServiceClientSecret}");
            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            using var response = await httpClient.PostAsync(_appConfig.IMSEmailSendBySendGridIdUrl, stringContent).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            else
            {
                var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var payload = JsonConvert.DeserializeObject<Response>(payloadString);

                if (payload.Code == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException(payload.Message);

                throw new HttpResponseException(payload.Code, payload.Message);
            }
        }
    }
}