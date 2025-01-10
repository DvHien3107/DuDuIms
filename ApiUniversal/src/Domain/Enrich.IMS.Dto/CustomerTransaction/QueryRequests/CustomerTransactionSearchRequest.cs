using Enrich.Dto.Base.Requests;
using Enrich.Dto.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.CustomerTransaction
{
    public class CustomerTransactionSearchRequest : BaseSearchWithFilterRequest<CustomerTransactionSearchCondition, CustomerTransactionFilterCondition>
    {
    }
}