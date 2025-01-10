using Enrich.IMS.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Library.Lookup
{
    public class LookupInfoManager
    {
        private readonly LookupInfoDataSource lookupInfoDataSource;

        public LookupInfoManager(LookupInfoDataSource lookupInfoDataSource)
        {
            this.lookupInfoDataSource = lookupInfoDataSource;
        }

        public async Task<Dictionary<string, LookupInfoDto>> GetLookupConfigAsync(GridType type)
        {
            var result = (int)type switch
            {
            
                (int)GridType.SearchSalesLead => await lookupInfoDataSource.GetSaleLeadLookupConfigAsync(),
                (int)GridType.SearchTicket => await lookupInfoDataSource.GetSaleLeadLookupConfigAsync(),
                (int)GridType.SearchMerchant => await lookupInfoDataSource.GetSaleLeadLookupConfigAsync(),
                _ => null
            };
            return result;
        }

    }
}
