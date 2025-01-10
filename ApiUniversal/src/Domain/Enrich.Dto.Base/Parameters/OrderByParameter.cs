using Enrich.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Parameters
{
    public class OrderByParameter
    {
        /// <summary>
        /// use for search on grid cte
        /// </summary>
        public string Name { set; get; }
        public SqlMapDto Field { set; get; }

        public SortDirectionEnum Direction { set; get; }

        public OrderByParameter()
        {
        }

        public OrderByParameter(SortDirectionEnum direction)
            : this()
        {
            Direction = direction;
        }

        public OrderByParameter(SortDirectionEnum direction, string name)
            : this(direction)
        {
            Field = new SqlMapDto(name);
            Name = name;
        }
    }
}
