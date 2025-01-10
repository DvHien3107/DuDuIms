using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IServices.Utils.GoHighLevelConnector
{
	public interface IGoHighLevelConnectorService
	{
		Task ChangeContactTypeToCustomerAsync(string salesLeadId);
		Task SyncingSalesLeadFromGoHighLevelAsync();
	}
}
