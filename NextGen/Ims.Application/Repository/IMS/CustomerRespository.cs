using Pos.Application.Extensions.Helper;
using Pos.Model.Model.Table;
using Pos.Model.Model.Table.IMS;
using Promotion.Application.Extensions;
using Promotion.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Repository.IMS
{
    public interface ICustomerRespository : IEntityService<C_Customer>
    {
        Task<C_Customer> getCustomer(string CustomerCode);
    }
    public class CustomerRespository : IMSEntityService<C_Customer>, ICustomerRespository
    {
        public async Task<C_Customer> getCustomer(string CustomerCode)
        {
            return await _connection.SqlFirstOrDefaultAsync<C_Customer>(" select * from C_Customer with (nolock) where CustomerCode=@CustomerCode", new { CustomerCode });
        }
    }
}
