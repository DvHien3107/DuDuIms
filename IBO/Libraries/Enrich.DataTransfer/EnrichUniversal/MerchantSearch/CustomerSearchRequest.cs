using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enrich.DataTransfer
{
    public class CustomerSearchRequest : BaseSearchWithFilterRequest<CustomerSearchCondition, CustomerSearchFilterCondition>
    {
        public CustomerSearchRequest()
        {
            Condition = new CustomerSearchCondition();
        }
    }
}