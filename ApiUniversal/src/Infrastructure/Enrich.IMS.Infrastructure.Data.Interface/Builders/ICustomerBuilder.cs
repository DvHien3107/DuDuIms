using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Builders
{
    public interface ICustomerBuilder
    {
        /// <summary>
        /// Builder data for create new Customer
        /// </summary>
        /// <param name="isNew">true if is new</param>
        /// <param name="customer">Customer data</param>
        public void BuildForSaveFromSalesLead(bool isNew, CustomerDto customer, SalesLeadDto salesLead);
    }
}
