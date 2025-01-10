using Enrich.DataTransfer;
using Enrich.DataTransfer.ClickUpConnector;
using Enrich.IServices;
using Enrich.IServices.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Enrich.Services.Utils
{
	public class ClickUpConnectorService : IClickUpConnectorService
	{
		private readonly string _apiUrl;
		private readonly ILogService _logger;
		private readonly EnrichContext _enrichContext;

		public ClickUpConnectorService(EnrichContext enrichContext, ILogService logger)
		{
			_enrichContext = enrichContext;
			_logger = logger;
			_apiUrl = ConfigurationManager.AppSettings["ClickUpConnectorBaseUrl"];

		}


      
        public async Task SyncMerchantToClickUpAsync(string merchantId)
		{
    //            #if !DEBUG
				//using (var httpClient = new HttpClient())
				//{
				//	httpClient.BaseAddress = new Uri(_apiUrl);
				//	httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_enrichContext.AccessToken}");
				//	using (var response = await httpClient.PostAsync($"{_apiUrl}/ims-click-up-syncing/sync-to-clickup/{merchantId}",null))
				//	{
				//		string responseJson = string.Empty;
				//		if (response.StatusCode == HttpStatusCode.OK)
				//		{
				//			responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
							
				//		}
				//		else
				//		{
				//			responseJson = await response.Content.ReadAsStringAsync();
				//			//response.ToString();

    //                        throw new Exception(responseJson);
				//		}
				//	}
				//}
				//#endif
		}

		public async Task CreateTaskDeliveryToClickUpAsync(string invoiceId)
		{
			try
			{
				//#if !DEBUG
				//using (var httpClient = new HttpClient())
				//{
				//	httpClient.BaseAddress = new Uri(_apiUrl);
				//	httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_enrichContext.AccessToken}");
				//	using (var response = await httpClient.PostAsync($"{_apiUrl}/ims-click-up-syncing/clickup-task-delivery/{invoiceId}", null))
				//	{
				//		string responseJson = string.Empty;
				//		if (response.StatusCode == HttpStatusCode.OK)
				//		{
				//			responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

				//		}
				//		else
				//		{
				//			responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				//			throw new Exception(responseJson);
				//		}
				//	}
				//}
				//#endif
			}
			catch (Exception ex)
			{
				_logger.Error(ex, $"[ClickUpConnectorService] get field error");
			}
		}


		public async Task<string> GetMappingByIMSIdAsync(string merchantId)
		{
			try
			{
                return null;
                //using (var httpClient = new HttpClient())
                //{
                //	httpClient.BaseAddress = new Uri(_apiUrl);
                //	httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_enrichContext.AccessToken}");
                //	using (var response = await httpClient.PostAsync($"{_apiUrl}/ims-click-up-syncing/get-mapping-by-ims-id/{merchantId}", null))
                //	{
                //		string responseJson = string.Empty;
                //		if (response.StatusCode == HttpStatusCode.OK)
                //		{
                //			responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                //			return responseJson;
                //		}
                //		else
                //		{
                //			responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                //			return null;
                //		}
                //	}
                //}
            }
			catch (Exception ex)
			{
				_logger.Error(ex, $"[ClickUpConnectorService] get field error");
				return null;
			}
		}

	}
}
