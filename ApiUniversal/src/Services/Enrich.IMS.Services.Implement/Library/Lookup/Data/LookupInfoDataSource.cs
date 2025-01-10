using Enrich.IMS.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Library.Lookup
{

    public partial class LookupInfoDataSource
    {
        public async Task<Dictionary<string, LookupInfoDto>> GetSaleLeadLookupConfigAsync()
        {
            var result = new Dictionary<string, LookupInfoDto>() {

                { "Type", new LookupInfoDto("SaleType") },
                { "Status", new LookupInfoDto("SaleStatus") }
            };

            return result;
        }
    }
}
