using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IServices.Utils.JiraConnector
{
    public interface IJiraConnectorAuthService
    {
        Task<string> GetAuthorizeUrl();
        Task<bool> Auth(string code);
        Task<bool> CheckAuth();
    }
}
