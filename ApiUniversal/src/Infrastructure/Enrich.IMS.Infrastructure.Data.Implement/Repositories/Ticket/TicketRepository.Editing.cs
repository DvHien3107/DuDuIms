using Dapper;
using Dommel;
using Enrich.Common.Helpers;
using Enrich.Dto.Base.Parameters;
using Enrich.Dto.List;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Ticket;
using Enrich.IMS.Dto.Ticket.Queries;
using Enrich.IMS.Dto.Ticket.Reponses;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class TicketRepository
    {
        #region ticket status
        public async Task<bool> UpdateTicketStatusAsync(long ticketId, TicketStatus ticketStatus, bool isNew = false)
        {
            return await ExecuteBySharedAsync(async (con, tran) =>
            {
                var queryTop1 = $"SELECT TOP (1) * FROM {SqlTables.TicketStatusMapping} WHERE TicketId=@KeyValue";
                var statusMapping = await con.QueryFirstOrDefaultAsync<TicketStatusMapping>(queryTop1, new { KeyValue = ticketId }, transaction: tran);
                if (statusMapping != null)
                {
                    statusMapping.StatusId = ticketStatus.Id;
                    statusMapping.StatusName = ticketStatus.Name;
                    var result = await con.UpdateAsync(statusMapping, tran);
                    return result;
                }
                else
                {
                    statusMapping = new TicketStatusMapping
                    {
                        StatusId = ticketStatus.Id,
                        StatusName = ticketStatus.Name,
                        TicketId = ticketId
                    };
                    var result = await con.InsertAsync(statusMapping, tran);
                    return true;
                }
            });
        }
        #endregion
        #region ticket types
        public async Task<bool> UpdateTicketTypesAsync(long ticketId, List<TicketType> ticketTypes, bool isNew = false)
        {
            return await ExecuteBySharedAsync(async (con, tran) =>
            {
                var clearTypes = $"DELETE FROM {SqlTables.TicketTypeMapping} WHERE TicketId=@KeyValue";
                await con.ExecuteAsync(clearTypes, new { KeyValue = ticketId }, transaction: tran);
                if (ticketTypes!=null && ticketTypes.Count>0)
                {
                    foreach (var ticketType in ticketTypes)
                    {
                       var typeMapping = new TicketTypeMapping
                        {
                            TypeId = ticketType.Id,
                            TypeName = ticketType.TypeName,
                            TicketId = ticketId
                       };
                        var result = await con.InsertAsync(typeMapping, tran);
                    }
                }
                return true;
            });
        }
        #endregion
    }
}
