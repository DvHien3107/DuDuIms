using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Builders
{
    public interface ICustomerTransactionBuilder
    {
        public CustomerTransaction BuildForFree(Order order);
        public CustomerTransaction BuildForCreditCard(Order order, CustomerCard card);
        public CustomerTransaction BuildForACH(Order order, Customer customer);
    }
}
