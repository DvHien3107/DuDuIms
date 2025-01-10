using System;

namespace Enrich.IMS.Entities
{
    public class NewCustomerGoal
    {
        public int Id { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }

        public int Goal { get; set; }

        public string Note { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; }

        public int? SiteId { get; set; }
    }
}
