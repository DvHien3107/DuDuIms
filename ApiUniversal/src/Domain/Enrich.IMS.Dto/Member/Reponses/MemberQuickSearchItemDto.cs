using Enrich.Dto.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Member
{
    public class MemberQuickSearchItemDto
    {
        [SqlMapDto(SqlTables.Member + ".Id")]
        public int Id { get; set; }

        /// <summary>
        /// Member number
        /// </summary>
        [SqlMapDto(SqlTables.Member + ".MemberNumber")] 
        public string MemberNumber { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        [SqlMapDto(SqlTables.Member + ".FirstName")] 
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        [SqlMapDto(SqlTables.Member + ".LastName")] 
        public string LastName { get; set; }

        /// <summary>
        /// Full name
        /// </summary>
        [SqlMapDto(SqlTables.Member + ".FullName")] 
        public string FullName { get; set; }

        /// <summary>
        /// login email, Email come from enrich domain
        /// </summary>
        [SqlMapDto(SqlTables.Member + ".PersonalEmail")] 
        public string PersonalEmail { get; set; }
       
    }
}
