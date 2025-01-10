using Pos.Application.Repository.IMS;
using Pos.Model.Model.Table.IMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.DBContext
{
    public interface ITransactionService
    {
        Task<string> insertCustomerTrans(C_CustomerTransaction insert);
    }
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepo;
        
        public TransactionService(ITransactionRepository transactionRepo)
        {
            _transactionRepo = transactionRepo;
        }



        public async Task<string> insertCustomerTrans(C_CustomerTransaction insert)
        {
            return await _transactionRepo.insertCustomerTrans(insert);
        }
    }
}
