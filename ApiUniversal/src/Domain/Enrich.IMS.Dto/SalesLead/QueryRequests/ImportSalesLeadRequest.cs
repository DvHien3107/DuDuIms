using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.SalesLead
{
    public class ImportSalesLeadRequest
    {     
        /// <summary>
        /// stream, use to import data
        /// </summary>
        public Stream ExcelContent { get; set; }
    }
}
