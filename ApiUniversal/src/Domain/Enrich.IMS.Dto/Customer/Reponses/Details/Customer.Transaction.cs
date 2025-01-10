using System.Collections.Generic;

namespace Enrich.IMS.Dto.Customer
{
    public partial class CustomerDetailResponse
    {
        public IEnumerable<CustomerTransactionDto> Transactions { get; set; }
        public IEnumerable<CustomerCardDto> Cards { get; set; }
    }
}