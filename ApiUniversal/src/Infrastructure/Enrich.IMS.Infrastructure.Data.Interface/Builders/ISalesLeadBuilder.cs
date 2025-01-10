using Enrich.IMS.Dto;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Builders
{
    public interface ISalesLeadBuilder
    {
        /// <summary>
        /// Builder data for create new Sales Lead
        /// </summary>
        /// <param name="salesLead">Sales lead data</param>
        Task BuildForSave(bool isNew, SalesLeadDto salesLead);
        Task BuildForImport(SalesLeadDto salesLead);
    }
}
