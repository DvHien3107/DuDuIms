using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Table.RCP
{
    [Table("IMSRequestlog")]
    public class IMSRequestlog
    {
        [Key]
        public int ID { get; set; }
        public string ActionName { get; set; }
        public string Request { get; set; }
        public DateTimeOffset TimeRequest { get; set; }
    }
}
