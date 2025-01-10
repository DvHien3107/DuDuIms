using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.SMS.IMS.Models
{
    public class NotificationSMSModel
    {
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public List<Uri> MediaUrl { get; set; }
    }

    public class ResNotification
    {
        public int TotalSuccess { get; set; }
        public int TotalFailed { get; set; }
        public string Message { get; set; }
    }
}
