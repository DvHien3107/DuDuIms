using System.Collections.Generic;

namespace Enrich.IMS.Dto.Customer
{
    public partial class CustomerDetailResponse
    {
        public IEnumerable<UploadMoreFileDto> Files { get; set; }
    }
}