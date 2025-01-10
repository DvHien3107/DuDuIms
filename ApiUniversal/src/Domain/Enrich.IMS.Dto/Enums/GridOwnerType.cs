using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto
{
    public enum GridOwnerType
    {
        /// <summary>
        /// UserId > 0 
        /// </summary>
        User = 1,    
        /// <summary>
        /// UserId = 0 (null)
        /// </summary>
        GlobalUser = 2
    
    }
}
