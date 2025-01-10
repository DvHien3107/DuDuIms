using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto
{
    public class SaveWebGridConfigRequest
    {
        public GridConfigDto Config { get; set; }

        public GridOwnerType? OwnerType { get; set; }

        public int? FilterId { get; set; }

        /// <summary>
        /// Only apply for OwnerType = GlobalFilter
        /// </summary>
        public int[] FilterIds { get; set; }

        /// <summary>
        /// Only apply for OwnerType = GlobalUser
        /// </summary>
        public int[] UserIds { get; set; }

        #region Internal

        public GridType Type { get; set; }

        public long UserId { get; set; }

        public string ConfigAsJson { get; set; }

        #endregion

        public SaveWebGridConfigRequest(GridType type, GridOwnerType ownerType)
        {
            Type = type;
            OwnerType = ownerType;
        }

        public SaveWebGridConfigRequest()
        {
        }
    }
}
