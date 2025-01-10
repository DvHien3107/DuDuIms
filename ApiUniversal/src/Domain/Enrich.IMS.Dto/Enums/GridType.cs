using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto
{
    public enum GridType
    {
        None = 0,

        SearchSalesLead = 10,

        SearchSalesLeadComment = 11,

        SearchMerchant = 20,

        ReportActiveMerchant = 21,

        ReportCanceledMerchant = 22,

        SearchTicket = 30
    }
}
