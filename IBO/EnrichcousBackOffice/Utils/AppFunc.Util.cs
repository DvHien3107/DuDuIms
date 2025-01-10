using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;

namespace EnrichcousBackOffice.Utils
{
    public static partial class AppFunc
    {
        /// <summary>
        /// Check role of user : 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CanAccess(string key)
        {
            Dictionary<string, bool> access = Authority.GetAccessAuthority();
            if (access.Any(k => k.Key.Equals(key)) && access[key] == true)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Check role of user : 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CanAccess(AccessRole key)
        {
            return CanAccess(key.Text());
        }
    }
}