using Enrich.DataTransfer.EnrichUniversal.NewCustomerGoal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IServices.Utils.EnrichUniversal
{
    public interface INewCustomerGoalService
    {
        /// <summary>
        /// Get New Customer Goal List
        /// </summary>
        /// <param name="request"></param>
        /// <returns>NewCustomerGoalResponseDto</returns>
        Task<NewCustomerGoalResponseDto> GetNewCustomerGoalList(NewCustomerGoalRequestDto request);

        /// <summary>
        /// Create New Customer Goal
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task Create(NewCustomerGoalDto request);

        /// <summary>
        /// Update New Customer Goal
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task PatchUpdate(List<NewCustomerGoalUpdateRequest> request, object Id);
    }
}
