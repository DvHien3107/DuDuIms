using System.Collections.Generic;

namespace Enrich.IMS.Dto.Customer
{
    public partial class CustomerDetailResponse
    {
        public IEnumerable<CustomerSupportingInfoDto> SupportingInfos { get; set; }
        public IEnumerable<SalesLeadCommentDto> Comments { get; set; }
        public IEnumerable<SalesLeadLogDto> Logs { get; set; }
    }
}
