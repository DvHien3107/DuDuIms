using Dapper;
using Pos.Model.Model.Table.POS;
using Promotion.Application.Extensions;
using Promotion.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Repository
{
    public interface IEmailRepository : IEntityService<RDDefRVCList>
    {
        Task<string?> getSendGridApiKey();
        Task<string?> getEmailAddress();
    }
    public class EmailRepository : POSEntityService<RDDefRVCList>, IEmailRepository
    {
        public async Task<string?> getSendGridApiKey()
        {
            return (await _connection.QueryFirstOrDefaultAsync<string>("select ParaStr from RDPara with (nolock) where RVCNo=0 and ParaName='SendGridApiKey'"));
        }

        public async Task<string?> getEmailAddress()
        {
            return (await _connection.QueryFirstOrDefaultAsync<string>("select ParaStr from RDPara with (nolock) where RVCNo=0 and ParaName='EmailAddress'"));
        }
    }
}
