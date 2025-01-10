using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Services.Utils.SMS
{
    public enum HistorySMSTypeEnum
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
}
