using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Common
{
    public class LoadOrUpdateBaseOption
    {
        #region Custom

        internal bool ForUpdate { get; set; }

        internal bool ForPreview { get; set; }

        #endregion

    }
}
