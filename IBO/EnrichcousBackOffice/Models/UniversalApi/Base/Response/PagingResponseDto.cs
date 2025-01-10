using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.UniversalApi.Base
{
    public class PagingResponseDto<T>
    {
        public PagingResponseDto()
        {
            Records = new List<T>();
            Pagination = new PaginationDto();
        }
        /// <summary>
        /// records
        /// </summary>
        public IEnumerable<T> Records { get; set; }
        /// <summary>
        /// pagination
        /// </summary>
        public PaginationDto Pagination { get; set; }
    }
}