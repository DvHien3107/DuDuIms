using Dapper;
using Pos.Model.Model.Table;
using Pos.Model.Model.Table.IMS;
using Promotion.Application.Extensions;
using Promotion.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Repository
{
    public interface ILoginRepository : IEntityService<P_Member>
    {
        Task<P_Member?> doLogin(string email, string pass);
    }

    public class LoginRepository : IMSEntityService<P_Member>, ILoginRepository
    {
        public async Task<P_Member?> doLogin(string email, string pass)
        {
            return await _connection.QueryFirstOrDefaultAsync<P_Member>("EXEC P_Login @email, @pass", new { email, pass });
        }
    }
}
