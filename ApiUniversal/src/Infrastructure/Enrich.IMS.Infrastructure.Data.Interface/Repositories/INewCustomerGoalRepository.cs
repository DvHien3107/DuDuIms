using Enrich.Core.Infrastructure.Repository;
using Enrich.Dto.Base.POSApi;
using Enrich.IMS.Dto.NewCustomerGoal;
using Enrich.IMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface INewCustomerGoalRepository : IGenericRepository<NewCustomerGoal>
    {
        Task<bool> IsExistGoalForTime(int year, int month);

        /// <summary>
        /// Get list Goal in year
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        Task<IEnumerable<NewCustomerGoal>> GetByYear(int year);

        Task SaveByTranAsync(bool isNew, NewCustomerGoal goal);

        Task<NewCustomerGoalSearchResponse> SearchAsync(NewCustomerGoalSearchRequest request);
    }
}