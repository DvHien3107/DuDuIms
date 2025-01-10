using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Entities
{
    public class MemberAccessKey
    {
        public int Id { get; set; }
        public long MemberId { get; set; }
        public string SecretKey { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime DeletedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
