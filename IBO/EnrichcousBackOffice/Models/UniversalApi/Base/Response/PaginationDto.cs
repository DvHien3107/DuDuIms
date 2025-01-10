using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.UniversalApi.Base
{
    public class PaginationDto
    {   /// <summary>
        /// page index
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// page size
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// page count
        /// </summary>
        public int PageCount { get; set; }
        /// <summary>
        /// total record
        /// </summary>
        public int TotalRecords { get; set; }
    }
}