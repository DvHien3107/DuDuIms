using Dapper;
using Pos.Application.DBContext;
using Pos.Model.Model.Table.RCP;
using Promotion.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Repository
{
    public interface IRcpRepository : IEntityService<Store>
    {
        public long insertStore(Store item);
        Task<IEnumerable<Store>> getStoreByLogin(string login);
        Task<string> CreateOtp(string EmpEmail, string StrDateTimeOTP, string TypeOTP);
        Task<string> getStrTime_LoginVerifyCode(string MailCheck, string vrfCode);
        Task<int> insertSupportLoginLog(string MailCheck, string StrDateOTP, string LogReason, string logDesc, int sectionTime, string LoginId);
        Task updateLoginVerifyCode(string MailCheck, string vrfCode);
    }
    public class RcpRepository : RCPEntityService<Store>, IRcpRepository
    {
        public long insertStore(Store item)
        {
            return _connection.Insert<long, Store>(item);
        }

        public async Task<IEnumerable<Store>> getStoreByLogin(string login)
        {
            return await _connection.QueryAsync<Store>("exec P_FindStoreByEmail @login", new { login });
        }

        public async Task<string> CreateOtp(string EmpEmail, string StrDateTimeOTP, string TypeOTP)
        {
            return await _connection.QueryFirstOrDefaultAsync<string>("exec P_CreateOTPCode @EmpEmail, @StrDateTimeOTP, @TypeOTP", new { EmpEmail, StrDateTimeOTP, TypeOTP });
        }

        public async Task<string> getStrTime_LoginVerifyCode(string MailCheck, string vrfCode)
        {
            return (await _connection.QueryFirstOrDefaultAsync<string>($"select StrTime from LoginVerifyCode with (nolock) where EmployeeEmail=@MailCheck and VeriyCode=@vrfCode and IsUsedVrfCode=0", new { MailCheck, vrfCode }));
        }

        public async Task<int> insertSupportLoginLog(string MailCheck, string StrDateOTP, string LogReason, string logDesc, int sectionTime, string LoginId)
        {
            return await _connection.QueryFirstOrDefaultAsync<int>("insert into  SupportLoginLog(EmpVerifyEmail, LogStrTime, LogSpendTime, LogReason, LogDesc, StrStatus, LoginId) OUTPUT Inserted.LoginLogId values(@MailCheck, @StrDateOTP, @sectionTime, @LogReason,@logDesc, 'LOGIN', @LoginId)", new { MailCheck, StrDateOTP, sectionTime, LogReason, logDesc, LoginId });
        }

        public async Task updateLoginVerifyCode(string MailCheck, string vrfCode)
        {
            await _connection.ExecuteAsync("update  LoginVerifyCode set IsUsedVrfCode = 1  where EmployeeEmail=@MailCheck and VeriyCode=@vrfCode and IsUsedVrfCode=0", new { MailCheck, vrfCode });
        }
    }
}
