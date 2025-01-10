using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public class CustomerGiftcardRepository : DapperGenericRepository<CustomerGiftcard>, ICustomerGiftcardRepository
    {
        public CustomerGiftcardRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }
    }
}
