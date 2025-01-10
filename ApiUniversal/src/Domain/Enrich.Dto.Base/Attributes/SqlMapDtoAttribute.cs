using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Attributes
{
    public class SqlMapDtoAttribute : Attribute
    {
        public string SqlName { get; }

        public string DtoName { get; }

        public bool UseToSort { get; set; }

        /// <summary>
        /// Always appear in select
        /// </summary>
        public bool IsRequired { get; set; }

        #region Custom

        public string SqlJoinKeys { get; }

        public bool IsTemplateField { get; set; }

        #endregion

        public SqlMapDtoAttribute(string sqlName, string dtoName = "", string sqlJoinKeys = "")
        {
            SqlName = sqlName;
            DtoName = dtoName;
            SqlJoinKeys = sqlJoinKeys;
        }
    }
}
