using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface ICustomerCardService : IGenericService<CustomerCard, CustomerCardDto>
    {
        Task<string> GetRecurringCardAsync(string customerCode);
    }
}