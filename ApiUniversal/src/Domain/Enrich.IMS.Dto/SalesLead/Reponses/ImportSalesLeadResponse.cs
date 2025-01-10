using Enrich.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.SalesLead
{
    public class ImportSalesLeadResponse
    {
        public List<ImportResult> ErrorResults { get; set; } = new List<ImportResult>();

        public int TotalSucceedRows { get; set; }

        public int TotalFailRows { get; set; }

        public string ErrorMessage { get; set; }
        public string Status { get; set; } = ProcessStatus.Success.ToString();

        internal ImportResult AddResult(int rowIndex,string message = null)
        {
            var result = new ImportResult { RowIndex = rowIndex, Message = message };
            ErrorResults.Add(result);
            return result;
        }

        public class ImportResult
        {
            public int RowIndex { get; set; }

            /// <summary>
            /// success or not, if not, print the message error.
            /// </summary>
            public string Message { get; set; }
        }
    }
}
