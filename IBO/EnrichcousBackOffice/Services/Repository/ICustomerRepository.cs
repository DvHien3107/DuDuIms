using Enrich.Entities;
using EnrichcousBackOffice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnrichcousBackOffice.Services.Repository
{
    public interface ICustomerRepository
    {
        Models.C_Customer getCustomerByCode(string customerCode);
    }
    public class CustomerRepository : ICustomerRepository
    {
        public Models.C_Customer getCustomerByCode(string customerCode)
        {
            using(WebDataModel db = new WebDataModel())
            {

                var orderProduct = db.C_Customer.FirstOrDefault(o => o.CustomerCode == customerCode);
                return orderProduct;
            }
        }
    }
}
