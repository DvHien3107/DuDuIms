using Pos.Model.Model.Table.IMS.Views;
using Pos.Model.Model.Table.IMS;
using Promotion.Application.Extensions;
using Promotion.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pos.Application.Extensions.Helper;

namespace Pos.Application.Repository.IMS
{
    public interface ITicketRepository : IEntityService<Store_Services>
    {
        Task<List<LoadTicket>> LoadTicket(string DepartmentID, string TypeId, DateTime start, DateTime end);
        Task<List<T_Tags>> getAllTags();
    }
    public class TicketRepository : IMSEntityService<Store_Services>, ITicketRepository
    {
        public async Task<List<T_Tags>> getAllTags()
        {
            return (await _connection.SqlQueryAsync<T_Tags>(" select * from T_Tags with (nolock) ")).ToList();
        }
        public async Task<List<LoadTicket>> LoadTicket(string DepartmentID, string TypeId, DateTime start, DateTime end)
        {
            return (await _connection.SqlQueryAsync<LoadTicket>("exec P_LoadTicket @DepartmentID, @TypeId, @start, @end", new { DepartmentID, TypeId, start, end })).ToList();
        }
    }
}
