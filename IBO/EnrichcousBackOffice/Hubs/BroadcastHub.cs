using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

//using System.Configuration;
//using EnrichcousBackOffice.Models.CustomizeModel;
//using TableDependency.SqlClient;
//using TableDependency.SqlClient.Base.EventArgs;
//using TableDependency.SqlClient.Base.Enums;

namespace EnrichcousBackOffice
{
    public class BroadcastHub : Hub
    {
        public void Send(string name, string message)
        {
            Clients.All.BroadcastNotice(name, message);
        }
        public void sendNotificationByMemberNumber(List<string> MemberNumbers, int NotificationId,string EntityId,int TemplateId)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<BroadcastHub>();
            context.Clients.Users(MemberNumbers).pushNotification(NotificationId.ToString(), EntityId, TemplateId);
        }
        public void completeExportFiles(List<string> MemberNumbers)
        {
            IHubContext context = GlobalHost.ConnectionManager.GetHubContext<BroadcastHub>();
            context.Clients.Users(MemberNumbers).completeProcessSMSExportFiles();
        }

        public static ConcurrentDictionary<string, string> dicStore = new ConcurrentDictionary<string, string>();
        public void NotifySubcribe(string key, string id)
        {
            if (dicStore.Count > 0 && dicStore.Any(d=>d.Key.Equals(key)) == true)
            {
                dicStore.TryUpdate(key, id, dicStore[key]);
            }
            else
            {
                dicStore.TryAdd(key, id);
            }


        }
        //client(connectionid).specifyNotice

        #region sqltabledependency

        //public void Dependency()
        //{

        //    ///
        //    string _con = new Models.WebDataModel().Database.Connection.ConnectionString;
        //    using (var dep = new SqlTableDependency<TicketDependencyModel>(_con, "T_SupportTicket"))
        //    {
        //        dep.OnChanged += OnChanged;
        //        dep.OnError += OnError;
        //        dep.OnStatusChanged += OnStatusChanged;
        //        dep.Start();

        //        System.Diagnostics.Debug.WriteLine("");
        //        System.Diagnostics.Debug.WriteLine("Waiting for receiving notifications (db objects naming: " + dep.DataBaseObjectsNamingConvention + ")...");

        //    }

        //}


        //private void OnStatusChanged(object sender, StatusChangedEventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine(Environment.NewLine);
        //    System.Diagnostics.Debug.WriteLine("");
        //    System.Diagnostics.Debug.WriteLine($"SqlTableDependency Status = {e.Status.ToString()}");

        //}

        //private void OnError(object sender, ErrorEventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine(Environment.NewLine);
        //    System.Diagnostics.Debug.WriteLine(e.Message);
        //    System.Diagnostics.Debug.WriteLine(e.Error?.Message);
        //}

        //private void OnChanged(object sender, RecordChangedEventArgs<TicketDependencyModel> e)
        //{

        //    System.Diagnostics.Debug.WriteLine($"SqlTableDependency Status OnChanged = {e.ChangeType.ToString()}");

        //    if (e.ChangeType != ChangeType.None)
        //    {
        //        var changedEntity = e.Entity;
        //        System.Diagnostics.Debug.WriteLine("Id: " + changedEntity.Id);
        //        System.Diagnostics.Debug.WriteLine("Name: " + changedEntity.Name);
        //        System.Diagnostics.Debug.WriteLine("Opened by: " + changedEntity.CreateByName);
        //        System.Diagnostics.Debug.WriteLine("At: " + changedEntity.CreateAt);
        //    }

        //}
        #endregion

    }
}