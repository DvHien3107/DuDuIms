using Enrich.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Parameters
{
    public class QueryParameter : BaseParameter
    {
        public List<SqlMapDto> Fields { get; set; }

        public PagingParameter Paging { get; set; }

        public List<OrderByParameter> OrderBy { set; get; }

        public ListViewMode ViewMode { get; set; } = ListViewMode.Grid;

        public bool IsOnlyGetTotalRecords { get; set; }

        #region Common

        public bool HasPaging => (Paging != null && Paging.PageSize != int.MaxValue) || IsOnlyGetTotalRecords;

        public List<string> GetJoinKeys()
        {
            var allFields = new List<SqlMapDto>(Fields.Where(a => !string.IsNullOrWhiteSpace(a.SqlJoinKeys)));
            if (OrderBy != null)
            {
                allFields.AddRange(OrderBy.Where(a => a.Field != null && !string.IsNullOrWhiteSpace(a.Field.SqlJoinKeys)).Select(a => a.Field));
            }

            return allFields
                .SelectMany(a => a.SqlJoinKeys.Split())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Select(a => a.Trim())
                .Distinct()
                .ToList();
        }

        #endregion

    }
}
