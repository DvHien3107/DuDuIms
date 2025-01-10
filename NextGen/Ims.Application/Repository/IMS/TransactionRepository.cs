using Dapper;
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
    public interface ITransactionRepository : IEntityService<C_CustomerTransaction>
    {
        Task<string> insertCustomerTrans(C_CustomerTransaction insert);
    }
    public class TransactionRepository :IMSEntityService<C_CustomerTransaction>, ITransactionRepository
    {
        public async Task<string> insertCustomerTrans(C_CustomerTransaction insert)
        {
            return await _connection.InsertAsync<string, C_CustomerTransaction>(insert);
        }
    }
}
