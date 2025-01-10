using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Table.RCP
{
    [Table(nameof(DatabaseControl))]
    public class DatabaseControl
    {
        public int ID { get; set; }
        public string DatabaseName { get; set; }
        public string IPServer { get; set; }
        public string Password { get; set; }
        public string UrlPOS { get; set; }
        public string UrlCheckIn { get; set; }
    }
}
