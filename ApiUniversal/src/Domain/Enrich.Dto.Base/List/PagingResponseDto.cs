using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.List
{
    public class PagingResponseDto<T>
    {
        public IEnumerable<T> Records { get; set; }

        public PaginationDto Pagination { get; set; }
    }
}
