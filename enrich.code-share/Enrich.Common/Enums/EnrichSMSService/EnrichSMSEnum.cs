using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Common.Enums
{
    public class EnrichSMSEnum
    {
        public enum HistoryType
        {
            DirectSMS = 10,
            ScheduleSMS = 20,
            DirectCampaign = 30,
            ScheduleCampaign = 40,
            InitialStore = 100,
            BuySMSPackage = 200,
            DeactiveSMS = 300,
            SyncRemaining = 400,
            SyncPosData = 500,
        }

        public class HistoryDescription
        {
            public const string InitialStore = "sync data from IMS";
            public const string DirectSMS = "send a SMS";
            public const string ScheduleSMS = "send a schedule SMS";
            public const string DirectCampaign = "send a campaign SMS";
            public const string ScheduleCampaign = "send a schedule campaign SMS";
            public const string BuySMSPackage = "bought a SMS package";
            public const string DeactiveSMS = "deactivated SMS package";
            public const string SyncPosData = "sync store data from POS";
        }
    }
}
