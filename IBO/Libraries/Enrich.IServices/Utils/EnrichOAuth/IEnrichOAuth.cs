using Enrich.DataTransfer;
using Enrich.DataTransfer.EnrichOAuth;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IServices.Utils.OAuth
{
    public interface IEnrichOAuth
    {
        Task<MemberContext> Validation(string email, string password);

        string GetAccessToken(string email, string password);
    }
}
