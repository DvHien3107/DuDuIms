using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;

namespace EnrichcousBackOffice.Extensions
{
    public static class EntityExtension
    {
        public static async Task<List<T>> ToListWithNoLockAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default)
        {
            List<T> result = default;
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                                    new TransactionOptions()
                                    {
                                        IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                                    },
                                    TransactionScopeAsyncFlowOption.Enabled))
            {
                result = await query.ToListAsync(cancellationToken);
                scope.Complete();
            }
            return result;
        }
    }
}