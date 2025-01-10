using Enrich.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Common
{
    /// <summary>
    /// search by special column on grid
    /// </summary>
    public class BaseFilterConditionRequestDto
    {
        public List<FieldFilterDetail> Fields { get; set; }
    }

    public class FieldFilterDetail
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public DataType DataType { get; set; } = DataType.String;

        public FilterType FilterType { get; set; } = FilterType.Contain;
    }
}
