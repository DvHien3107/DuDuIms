using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Core.Utils
{
    public interface IJiraConnectorService
    {
        /// <summary>
        /// Create jira issue from ticket id
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        Task CreateIssue(long ticketId);

        /// <summary>
        /// Create jira issue from ticket id
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        Task UpdateIssue(long ticketId);
        
    }
}
