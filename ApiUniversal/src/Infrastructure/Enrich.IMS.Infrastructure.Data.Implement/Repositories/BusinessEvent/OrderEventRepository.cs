using Dapper;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class OrderEventRepository : DapperGenericRepository<OrderEvent>, IOrderEventRepository
    {
        public OrderEventRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }
    }
}