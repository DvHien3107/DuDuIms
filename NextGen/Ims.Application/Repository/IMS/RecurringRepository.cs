using Pos.Application.Extensions.Helper;
using Pos.Model.Model.Proc.IMS;
using Pos.Model.Model.Table.IMS;
using Pos.Model.Model.Table.POS;
using Promotion.Application.Extensions;
using Promotion.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Repository.IMS
{
    public interface IRecurringRepository : IEntityService<Order_Subcription>
    {
        Task<List<Store_Services>> getListService();
        Task<List<P_GetRecurringList>> getListRecurringData();
    }

    public class RecurringRepository : IMSEntityService<Order_Subcription>, IRecurringRepository
    {
        public async Task<List<Store_Services>> getListService()
        {
            return (await _connection.SqlQueryAsync<Store_Services>("EXEC P_GetListRecurring")).ToList();
        }

        public async Task<List<P_GetRecurringList>> getListRecurringData()
        {
            return (await _connection.SqlQueryAsync<P_GetRecurringList>("EXEC P_GetRecurringList")).ToList();
        }
    }
}
