using Dapper;
using Dommel;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto.Base;
using Enrich.Dto.Base.Parameters;
using Enrich.Dto.List;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.SalesLead;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class SalesLeadRepository
    {

        public async Task SaveSalesLeadByTranAsync(bool isNew, SalesLead salesLead, Func<string, Task> extendSave = null)
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
                            var newId = await AddAsync(salesLead);
                            // salesLead.Id = salesLead.Id;
                        }
                        else
                        {
                            var affect = await UpdateAsync(salesLead);
                            if (affect == 0)
                                throw new Exception("Cannot save property");
                        }

                        if (extendSave != null)
                        {
                            await extendSave(salesLead.Id);
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

        #region customer
        public async Task<bool> SaveSaleLeadCustomerAsync(string customerCode, Customer customer, bool isNew = false)
        {
            return await ExecuteBySharedAsync(async (con, tran) =>
            {
                var queryTop1 = $"SELECT TOP (1) 1 FROM {SqlTables.Customer} WHERE customerCode=@KeyValue";
                var top1 = await con.QueryFirstOrDefaultAsync(queryTop1, new { KeyValue = customerCode }, transaction: tran);
                if (top1 != null)
                {
                    var result = await con.UpdateAsync(customer, tran);
                    return result;
                }
                else
                {
                    var result = await con.InsertAsync(customer, tran);
                    return true;
                }
            });
        }
        #endregion
    }
}