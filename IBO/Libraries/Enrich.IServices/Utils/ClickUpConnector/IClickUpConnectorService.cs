using Enrich.DataTransfer.ClickUpConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IServices.Utils
{
	public interface IClickUpConnectorService
	{
		Task SyncMerchantToClickUpAsync(string merchantId);

		Task CreateTaskDeliveryToClickUpAsync(string invoiceId);

		Task<string> GetMappingByIMSIdAsync(string merchantId);
	}
}
