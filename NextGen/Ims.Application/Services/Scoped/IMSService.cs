using Dapper;
using Pos.Application.DBContext;
using Pos.Application.Extensions.Helper;
using Pos.Model.Model.Proc;
using Pos.Model.Model.Table.IMS;
using Pos.Model.Model.Table.POS;
using Promotion.Application.Extensions;
using Promotion.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Services.Scoped
{
    public interface IIMSService : IEntityService<TwilioAccount>
    {
        Task<List<GetTollFree>> SyncTollFreeToPos();
        Task<string> getTollfreePhoneNumberSid(string phone);
        Task<List<GetNextWeekLicense>> getNextWeekLicense();
    }
    public class IMSService : IMSEntityService<TwilioAccount>, IIMSService
    {
        public async Task<List<GetTollFree>> SyncTollFreeToPos()
        {
            var ressult = await _connection.ImsAutoConnect().SqlQueryAsync<GetTollFree>("exec P_GetTollFree");
            return ressult.ToList();
        }
        public async Task<string> getTollfreePhoneNumberSid(string phone )
        {
            var ressult = await _connection.ImsAutoConnect().SqlQueryAsync<string>($"select TollfreePhoneNumberSid from TwilioA2PTollFreeVerification where PhoneNumber = '{phone}'");
            return ressult.FirstOrDefault()??"";
        }


        public async Task<List<GetNextWeekLicense>> getNextWeekLicense()
        {
            return (await _connection.ImsAutoConnect().SqlQueryAsync<GetNextWeekLicense>("exec P_GetNextWeekLicense")).ToList();
        }
    }
}
