using Enrich.Dto;
using System.Collections.Generic;

namespace Enrich.IMS.Dto.Lookup
{
    public class LookupDataRequest
    {
        /// <summary>
        /// the field need to search
        /// </summary>
        public string SearchField { get; set; }

        /// <summary>
        /// the value for searching by SearchField
        /// </summary>
        public string SearchText { get; set; }

        public int[] InIds { get; set; }

        /// <summary>
        /// ex: "xx DESC, yy ASC"
        /// </summary>
        public string SqlOrderby { get; set; }

        public LookupDataRequest(params int[] ids)
        {
            InIds = ids;
        }

        public string ExtendConditions { get; set; }

        public List<SqlMapDto> FieldNames { get; set; } = new List<SqlMapDto>();
    }
}