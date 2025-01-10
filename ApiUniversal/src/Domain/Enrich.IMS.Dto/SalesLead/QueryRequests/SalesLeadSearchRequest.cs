using Enrich.Dto.Base.Requests;
using Enrich.Dto.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.SalesLead
{
    public class SalesLeadSearchRequest : BaseSearchWithFilterRequest<SalesLeadSearchCondition, SalesLeadFilterCondition>
    {
    }
}