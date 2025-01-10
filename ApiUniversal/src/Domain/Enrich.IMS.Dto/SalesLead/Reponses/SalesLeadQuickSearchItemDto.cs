using Enrich.Dto.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.SalesLead
{
    public class SalesLeadQuickSearchItemDto 
    {
     
        [SqlMapDto(SqlTables.SalesLead + ".Id")]
        public string Id { get; set; }
        
        [SqlMapDto(SqlTables.SalesLead + ".CustomerCode")]
        public string CustomerCode { get; set; }      
       
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.SalonName}")]
        public string SalonName { get; set; }      


        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.Phone}")]
        public string SalonPhone { get; set; }


        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.ContactName}")]
        public string ContactName { get; set; }

       
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.ContactPhone}")]
        public string ContactPhone { get; set; }
       
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.Email}")]
        public string SalonEmail { get; set; }
    
    }
}
