using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;


namespace EnrichcousBackOffice
{
    public class SmsexportfileHub : Hub
    {
        public void Send(string name, string message)
        {
            Clients.All.BroadcastNotice(name, message);
        }
      
        public void completeExportFiles()
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<SmsexportfileHub>();
            context.Clients.All.completeProcessSMSExportFiles();
        }

        public static ConcurrentDictionary<string, string> dicStore = new ConcurrentDictionary<string, string>();
        public void NotifySubcribe(string key, string id)
        {
            if (dicStore.Count > 0 && dicStore.Any(d => d.Key.Equals(key)) == true)
            {
                dicStore.TryUpdate(key, id, dicStore[key]);
            }
            else
            {
                dicStore.TryAdd(key, id);
            }
        }    
    }
}