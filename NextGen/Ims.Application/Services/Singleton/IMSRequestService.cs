using Dapper;
using Microsoft.IdentityModel.Tokens;
using Pos.Application.DBContext;
using Pos.Application.Extensions.Helper;
using Pos.Model.Model.Auth;
using Pos.Model.Model.Table.RCP;
using Promotion.Application.Extensions;
using Promotion.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Services.Singleton
{
    public interface IIMSRequestService : IEntityService<Store>
    {
        int log(IMSRequestlog newlog);
        int store(Store newlog);
    }
    public class IMSRequestService : RCPEntityService<Store>, IIMSRequestService
    {
        public IMSRequestService()
        {

        }
        public IMSRequestService(IDbConnection db) : base(db)
        {

        }

        public int log(IMSRequestlog newlog)
        {
            return _connection.AutoConnect().Insert(newlog) ?? 0;
        }

        public int store(Store newlog)
        {
            return _connection.AutoConnect().Insert(newlog) ?? 0;
        }
    }
}
