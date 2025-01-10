using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.NewCustomerGoal;
using Enrich.IMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface INewCustomerGoalService : IGenericService<NewCustomerGoal, NewCustomerGoalDto>
    {
        Task<NewCustomerGoalDto> GetDetailAsync(int goalId);

        Task<NewCustomerGoalSearchResponse> SearchAsync(NewCustomerGoalSearchRequest request);

        Task<NewCustomerGoalUpdateResponse> CreateGoalAsync(NewCustomerGoalDto dto);

        Task<NewCustomerGoalUpdateResponse> UpdateGoalAsync(NewCustomerGoalUpdateRequest request);
    }
}
