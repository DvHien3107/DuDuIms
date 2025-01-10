using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public partial class DetailSalesLeadCustomizeModel
    {
       public IEnumerable<EnrichcousBackOffice.Models.Calendar_Event> even { get; set; }
       public EnrichcousBackOffice.ViewModel.SaleLeadInfo lead { get; set; }
    }
}