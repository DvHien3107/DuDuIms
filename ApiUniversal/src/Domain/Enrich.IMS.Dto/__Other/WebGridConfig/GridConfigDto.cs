using Enrich.Common.Enums;
using Enrich.IMS.Dto.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto
{
    public class GridConfigDto
    {
        public ListViewMode ViewMode { get; set; } = ListViewMode.Grid;

        public List<GridFieldConfigDto> Fields { get; set; } = new List<GridFieldConfigDto>();
    }

    public class GridFieldConfigDto
    {
        public string Name { get; set; }

        public string ColumnName { get; set; }

        public int Index { get; set; }

        public bool IsShow { get; set; }

        public SortDirectionEnum SortDirection { get; set; }

        public FixedColAlign FixedAlign { get; set; }

        public Dictionary<string, object> Style { get; set; } = new Dictionary<string, object>();

        public LookupInfoDto LookupInfo { get; set; }

        #region System

        public bool CanSort { get; set; }

        public bool CanSearch { get; set; }

        /// <summary>
        /// this field use for search on web app
        /// </summary>
        public string SearchName { get; set; }

        public string DataType { get; set; }

        public string DisplayType { get; set; }

        public string DisplayTypeFormat { get; set; }

        #endregion
    }

    /// <summary>
    /// use for searching
    /// </summary>
    public class LookupInfoDto
    {
        /// <summary>
        /// use to get data from lookup api 
        /// </summary>
        public string LookupType { get; set; }

        /// <summary>
        /// use to know which field want to search 
        /// </summary>
        public string SearchField { get; set; }

        /// <summary>
        /// may be mutilple value or single value
        /// </summary>
        public int SearchType { get; set; }

        public LookupInfoDto() { }

        public LookupInfoDto(string lookupType, string searchField = null, int searchType = (int)FieldSearchType.Single)
        {

            this.LookupType = lookupType;
            this.SearchField = searchField;
            this.SearchType = searchType;
        }
    }
}
