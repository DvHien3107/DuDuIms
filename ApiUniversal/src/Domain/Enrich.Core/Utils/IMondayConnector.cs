using Enrich.Dto.Base;
using System.Threading.Tasks;

namespace Enrich.Core.Utils
{
    public interface IMondayConnector
    {
        Task<bool> SyncingSalesLeadAsync();
    }
}
