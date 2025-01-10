using Enrich.DataTransfer;
using Enrich.IServices;
using Enrich.IServices.Utils.GoHighLevelConnector;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace Enrich.Services.Utils.GoHighLevelConnector
{
	public class GoHighLevelConnectorService : IGoHighLevelConnectorService
	{
		private readonly string _connectorBaseUrl;
		private readonly ILogService _logger;
		private readonly EnrichContext _enrichContext;
		public GoHighLevelConnectorService(ILogService logger, EnrichContext enrichContext)
		{
			_connectorBaseUrl = ConfigurationManager.AppSettings["GoHighLevelConnectorBaseUrl"];
			_logger = logger;
			_enrichContext = enrichContext;
		}

		public async Task ChangeContactTypeToCustomerAsync(string salesLeadId)
		{
			try
			{
				_logger.Info($"[GoHighLevelConnectorService] ChangeContactTypeToCustomerAsync with id: {salesLeadId}");
				//var _auth = $"Bearer {_enrichContext.AccessToken}";
				//if (string.IsNullOrEmpty(_enrichContext.AccessToken)) _auth = $"Bearer {_enrichContext.AccessToken}";
				//else _auth = $"Bearer {_enrichContext.AccessToken}";

				using (var httpClient = new HttpClient())
				{
					httpClient.BaseAddress = new Uri(_connectorBaseUrl);
					//httpClient.DefaultRequestHeaders.Add("Authorization", _auth);
					//var data = new StringContent(JsonConvert.SerializeObject(new ConnectorJiraAuthRequest { Code = code }));
					//data.Headers.ContentType = new MediaTypeHeaderValue("application/json");
					using (var response = await httpClient.PostAsync(string.Format($"{_connectorBaseUrl}/contact/mark_is_merchant/{salesLeadId}"), null))
					{
						if(response.StatusCode != System.Net.HttpStatusCode.OK)
						{
							string responseJson = await response.Content.ReadAsStringAsync();
							throw new Exception(responseJson);
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, $"[GoHighLevelConnectorService] ChangeContactTypeToCustomerAsync failed");
			
			}
		}


		public async Task SyncingSalesLeadFromGoHighLevelAsync()
		{
			try
			{
				_logger.Info($"[GoHighLevelConnectorService] SyncingSalesLeadFromGoHighLevelAsync start");
				//var _auth = $"Bearer {_enrichContext.AccessToken}";
				//if (string.IsNullOrEmpty(_enrichContext.AccessToken)) _auth = $"Bearer {_enrichContext.AccessToken}";
				//else _auth = $"Bearer {_enrichContext.AccessToken}";

				using (var httpClient = new HttpClient())
				{
					// httpClient.BaseAddress = new Uri(_connectorBaseUrl);
					//httpClient.DefaultRequestHeaders.Add("Authorization", _auth);
					//var data = new StringContent(JsonConvert.SerializeObject(new ConnectorJiraAuthRequest { Code = code }));
					//data.Headers.ContentType = new MediaTypeHeaderValue("application/json");
					string url = $"{_connectorBaseUrl}/sync-saleslead";
					using (var response = await httpClient.GetAsync(url))
					{
						if (response.StatusCode != System.Net.HttpStatusCode.OK)
						{
							string responseJson = await response.Content.ReadAsStringAsync();
							throw new Exception(responseJson);
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex, $"[GoHighLevelConnectorService] SyncingSalesLeadFromGoHighLevelAsync failed");
			}
		}
	}
}
