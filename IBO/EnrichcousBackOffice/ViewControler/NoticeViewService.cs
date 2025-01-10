using EnrichcousBackOffice.AppLB;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EnrichcousBackOffice.ViewControler
{
    public class NoticeViewService
    {


        /// <summary>
        /// gui thong bao all client
        /// </summary>
        /// <param name="name"></param>
        /// <param name="messge"></param>
        /// <returns></returns>
        public static bool BroadcastNotice(string name, string messge, string url = "/")
        {
            try
            {
                var hubContext = GlobalHost.ConnectionManager.GetHubContext<BroadcastHub>();
                hubContext.Clients.All.BroadcastNotice(name, messge, url);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        /// <summary>
        /// gui thong bao client  = alert popup voi Hubs.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="messge"></param>
        /// <param name="to">string membernumber vd: 0001,0002,1201,2548,...</param>
        /// <returns></returns>
        public static bool SpecifyNotice(string name, string messge, string url, string to)
        {
            try
            {
                var cmem = Authority.GetCurrentMember();
                to = to.Replace(cmem?.MemberNumber + ",", "");
                var dic = BroadcastHub.dicStore;
                var hubContext = GlobalHost.ConnectionManager.GetHubContext<BroadcastHub>();
                foreach (var item in to?.Split(new char[] { ',' }))
                {
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        continue;
                    }

                    if (dic.Count > 0 && string.IsNullOrWhiteSpace(dic[item]) == false)
                    {
                        hubContext.Clients.Client(dic[item]).SpecifyNotice(name, messge, url);
                    }
                }

                //hubContext.Clients.All.BroadcastNotice(name, messge);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }


        /// <summary>
        /// gui thong bao client thong qua web push notice.(service worker)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="messge"></param>
        /// <param name="to">string membernumber vd: 0001,0002,1201,2548,...</param>
        /// <returns></returns>
        public static string WebPushNotice(string name, string messge, string url, string to)
        {

            int? total = to?.Split(new char[] { ',' }).Count();
            int ok = 0;

            using (var db = new Models.WebDataModel())
            {
                try
                {
                    var cmem = Authority.GetCurrentMember();
                    to = to.Replace(cmem?.MemberNumber + ",", "");
                    foreach (var item in to?.Split(new char[] { ',' }))
                    {
                        if (string.IsNullOrWhiteSpace(item))
                        {
                            continue;
                        }


                        var subcriptionStr = db.P_Member.Where(m => m.MemberNumber == item).FirstOrDefault()?.NoticeSubscribeCode;
                        if (!string.IsNullOrWhiteSpace(subcriptionStr))
                        {

                            try
                            {
                                Dictionary<string, object> _subcription = JsonConvert.DeserializeObject<Dictionary<string, object>>(subcriptionStr);
                                string endpoint = _subcription["endpoint"].ToString();
                                Dictionary<string, object> key = JsonConvert.DeserializeObject<Dictionary<string, object>>(_subcription["keys"].ToString());
                                string auth = key["auth"].ToString();
                                string p256dh = key["p256dh"].ToString();

                                var subcription = new AppLB.PushNotice.Models.PushSubscription
                                {
                                    Endpoint = endpoint,
                                    Auth = auth,
                                    P256DH = p256dh
                                };

                                var payload = JsonConvert.SerializeObject(new PushMessageOption
                                {
                                    title = name,
                                    msg = messge,
                                    url = url,
                                    icon = "/content/img/icon.png"
                                });




                                AppLB.PushNotice.WebPushClient push = new AppLB.PushNotice.WebPushClient();
                                push.SetGcmApiKey(System.Configuration.ConfigurationManager.AppSettings["GcmApiKey"].ToString());
                                string publicKey = System.Configuration.ConfigurationManager.AppSettings["PublicKey"].ToString();
                                string privateKey = System.Configuration.ConfigurationManager.AppSettings["PrivateKey"].ToString();
                                push.SetVapidDetails("https://ims.enrichcous.com", publicKey, privateKey);
                                push.SendNotification(subcription, payload);

                                ok++;
                            }
                            catch (Exception)
                            {
                                continue;
                            }

                        }


                    }
                }

                catch (Exception)
                {

                }


            }

            return "sent:" + ok + "/" + total;

        }

    }


    /// <summary>
    /// Model push message
    /// </summary>
    public class PushMessageOption
    {
        public string title { get; set; }
        public string msg { get; set; }
        public string url { get; set; }
        public string icon { get; set; }

    }

}