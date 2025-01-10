using System.Collections.Generic;

namespace Enrich.IMS.Dto.Customer
{
    public partial class CustomerDetailResponse
    {
        public CustomerDto Profile { get; set; }
        public IEnumerable<CustomerContactDto> Contacts { get; set; }
    }
}
