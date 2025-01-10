using Dapper;
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
    public partial class TicketRepository : DapperGenericRepository<Ticket>, ITicketRepository
    {
        private const string AliasTicket = SqlTables.SupportTicket;
        public TicketRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }


        public async Task UpdateTicketAsync(bool isNew, Ticket ticket, Func<long, Task> extendSave = null)
        {
            //var queryChanges = GenerateSqlSaveChanges(new ChangeEntity<SalesLead>
            //{
            //    Added = isNew ? new List<SalesLead> { salesLead } : null,
            //    Modified = isNew ? null : new List<SalesLead> { salesLead }
            //});
            using (_connectionFactory.SharedConnection = GetDbConnection())
            {
                if (_connectionFactory.SharedConnection.State == ConnectionState.Closed)
                    _connectionFactory.SharedConnection.Open();

                using (_connectionFactory.SharedTransaction = _connectionFactory.SharedConnection.BeginTransaction()) // begin transaction
                {
                    try
                    {
                        if (isNew)
                        {
                            var newId = await AddAsync(ticket);
                        }
                        else
                        {
                            var affect = await UpdateAsync(ticket);
                            if (affect == 0)
                                throw new Exception("Cannot save property");
                        }

                        if (extendSave != null)
                        {
                            await extendSave(ticket.Id);
                        }

                        _connectionFactory.SharedTransaction.Commit();
                    }
                    catch (Exception)
                    {
                        _connectionFactory.SharedTransaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public async Task<TicketStatus> GetTicketStatusAsync(long ticketId)
        {
            var query = $"  SELECT TOP (1) T2.* FROM T_TicketStatusMapping T1 INNER JOIN T_TicketStatus T2 ON T1.StatusId = T2.Id WHERE T1.TicketId = @ticketId";

            using (var connection = GetDbConnection())
            {
                return await connection.QuerySingleOrDefaultAsync<TicketStatus>(query, new { ticketId = ticketId });
            }
        }
        public async  Task<IEnumerable<TicketType>> GetTicketTypesAsync(long ticketId)
        {
            var query = $"   SELECT  T2.* FROM T_TicketTypeMapping T1 INNER JOIN T_TicketType T2 ON T1.TypeId = T2.Id WHERE T1.TicketId = @ticketId";

            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<TicketType>(query, new { ticketId = ticketId });
            }
        }
        public async Task<IEnumerable<UploadMoreFile>> GetTicketAttachmentsAsync(long ticketId)
        {
            var query = $" SELECT * FROM UploadMoreFiles WHERE TableName = 'T_SupportTicket' AND TableId = @ticketId";

            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<UploadMoreFile>(query, new { ticketId = ticketId });
            }
        }
    }
}
