using Dapper;
using Pos.Application.DBContext;
using Pos.Application.Extensions.Helper;
using Pos.Model.Model.Proc;
using Pos.Model.Model.Table.IMS;
using Pos.Model.Model.Table.POS;
using Pos.Model.Model.Table.RCP;
using Promotion.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
//using Twilio.TwiML.Voice;

namespace Pos.Application.Services.Scoped
{
    public interface IRCPService : IEntityService<Store>
    {
        Task<Store> getStoresByImsId(string IMSID);
        Task<IEnumerable<Store>> getStoresByQuery(string Query);
        Task<DatabaseControl> getDatabaseControl();
        void updStore(Store newRVC);
        Task<List<ImsTollFree>> getCurrentTollFree();
        Task insertCurrentTollFree(GetTollFree model, string messageservice);
        Task<List<ImsTollFree>> getPendingTollFree();
        Task updateTollFree(string PhoneNumber, string MessageService);
        Task updatePhoneTollFree(string PhoneNumber, string PhoneService);
        Task updateExpDate(string IMSID, string ExpiryDate);
        Task<List<LstTollFreeToSync>> getLstTollFreeToSync();
        void insertRDTwillioResponse(RDTwillioResponse item);
        int insertStore(Store newRVC);
    }
    public class RCPService : RCPEntityService<Store>, IRCPService
    {
        public async Task<Store> getStoresByImsId(string IMSID)
        {
            return await _connection.RCPAutoConnect().SqlFirstOrDefaultAsync<Store>("select * from Store with (nolock) where MerchantID=@IMSID", new
            {
                 IMSID
            });
        }
        public void insertRDTwillioResponse(RDTwillioResponse item)
        {
            var check = checkBeforeInsert(item.smsid, item.AuthToken);
            if(check.Count == 0)
            {
                _connection.RCPAutoConnect().Insert(item);
            }
        }

        public List<int> checkBeforeInsert(string smsid, string AuthToken)
        {
            return ( _connection.RCPAutoConnect().SqlQuery<int>($"select top 1 1 from RDTwillioResponse with (nolock) where smsid='{smsid}' and AuthToken='{AuthToken}'")).ToList();
        }

        public async Task<List<LstTollFreeToSync>> getLstTollFreeToSync()
        {
            return (await _connection.RCPAutoConnect().SqlQueryAsync<LstTollFreeToSync>("exec LstTollFreeToSync")).ToList();
        }
        public async Task insertCurrentTollFree(GetTollFree model, string messageservice)
        {
            await _connection.RCPAutoConnect().ExecuteAsync($"insert into ImsTollFree (StoreCode, SId, AuthToken, PhoneNumber, MessageService,VerificationStatus) values('{model.StoreCode}','{model.SId}','{model.AuthToken}','{model.PhoneNumber}','{messageservice}','{model.VerificationStatus}')");
        }
        public async Task updateTollFree(string PhoneNumber, string MessageService)
        {
            await _connection.RCPAutoConnect().ExecuteAsync($"update ImsTollFree set MessageService = '{MessageService}' where PhoneNumber='{PhoneNumber}'");
        }

        public async Task updatePhoneTollFree(string PhoneNumber, string PhoneService)
        {
            await _connection.RCPAutoConnect().ExecuteAsync($"update ImsTollFree set PhoneMessageService = '{PhoneService}' where PhoneNumber='{PhoneNumber}'");
        }
        public async Task<List<ImsTollFree>> getCurrentTollFree()
        {
            return (await _connection.RCPAutoConnect().SqlQueryAsync<ImsTollFree>("select * from ImsTollFree with (nolock)")).ToList();
        }

        public async Task<List<ImsTollFree>> getPendingTollFree()
        {
            return (await _connection.RCPAutoConnect().SqlQueryAsync<ImsTollFree>("select * from ImsTollFree with (nolock) where MessageService ='not found'")).ToList();
        }
        public async Task<IEnumerable<Store>> getStoresByQuery(string Query)
        {
            return await _connection.RCPAutoConnect().SqlQueryAsync<Store>($"select * from Store with (nolock) where {Query}");
        }
        public async Task<DatabaseControl> getDatabaseControl()
        {
            return await _connection.RCPAutoConnect().SqlFirstOrDefaultAsync<DatabaseControl>("select top 1 * from DatabaseControl with (nolock) Order By ID Desc");
        }
        public void updStore(Store newRVC)
        {
            _connection.RCPAutoConnect().Update(newRVC);
        }
        public int insertStore(Store newRVC)
        {
           return _connection.RCPAutoConnect().Insert<int, Store>(newRVC);
        }
        public async Task updateExpDate(string IMSID, string ExpiryDate)
        {
             await _connection.RCPAutoConnect().ExecuteAsync($"update Store set ExpiryDate = '{ExpiryDate}' where IMSID='{IMSID}' and ExpiryDate<'{ExpiryDate}'");
        }
    } 
}
