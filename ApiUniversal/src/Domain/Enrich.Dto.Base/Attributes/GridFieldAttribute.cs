using Enrich.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Attributes
{
    public class GridFieldAttribute : Attribute
    {
        public int Index { get; set; }

        public bool CanSort { get; set; }

        public bool CanSearch { get; set; }

        public bool IsDefault { get; set; }

        public bool IsShow { get; set; }

        public FixedColAlign FixedAlign { get; set; }

        public string ColumnName { get; set; }

        public string DataType { get; set; }

        public string DisplayType { get; set; }

        public string DisplayFormat { get; set; }

        //custom
        public string Group { get; set; }

        public string RelatedDtoNames { get; set; }
    }
}
