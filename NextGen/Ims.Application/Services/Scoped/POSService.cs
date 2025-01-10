using Dapper;
using Pos.Application.DBContext;
using Pos.Application.Extensions.Helper;
using Pos.Model.Model.Proc;
using Pos.Model.Model.Table.POS;
using Pos.Model.Model.Table.RCP;
using Promotion.Application.Extensions;
using Promotion.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Services.Scoped
{
    public interface IPOSService : IEntityService<RDDefRVCList>
    {
        Task<RDDefRVCList> getDefRVCListsByImsId(string IMSID);
        int addDefRVCList(RDDefRVCList newRVC);
        Task excuteQuery(string query);
        void updDefRVCList(RDDefRVCList newRVC);
        Task<List<LstTollFreeToSync>> getLstTollFreeToSync();
    }
    public class POSService : POSEntityService<RDDefRVCList>, IPOSService
    {
        public async Task<RDDefRVCList> getDefRVCListsByImsId(string IMSID)
        {
            return await _connection.AutoConnect().SqlFirstOrDefaultAsync<RDDefRVCList>("select * from RDDefRVCList with (nolock) where IMSCode = @IMSID", new
            {
                IMSID = IMSID,
            });
        }

        public async Task<List<LstTollFreeToSync>> getLstTollFreeToSync()
        {
            return (await _connection.RCPAutoConnect().SqlQueryAsync<LstTollFreeToSync>("exec LstTollFreeToSync")).ToList();
        }
        public int addDefRVCList(RDDefRVCList newRVC)
        {
            return _connection.AutoConnect().Insert(newRVC) ?? 0;
        }
        public void updDefRVCList(RDDefRVCList newRVC)
        {
            _connection.AutoConnect().Update<RDDefRVCList>(newRVC);
        }
        public async Task excuteQuery(string query)
        {
            await _connection.AutoConnect().ExecuteAsync(query);
        }
    }
}
