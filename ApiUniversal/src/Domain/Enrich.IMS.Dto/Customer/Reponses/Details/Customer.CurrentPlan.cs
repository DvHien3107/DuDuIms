using System.Collections.Generic;

namespace Enrich.IMS.Dto.Customer
{
    public partial class CustomerDetailResponse
    {
        public IEnumerable<StoreServiceDto> StoreServices { get; set; }
        //public IEnumerable<RecurringPlanningDto> RecurringPlannings { get; set; }
    }
}