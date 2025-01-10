using System.Collections.Generic;
using System.Linq;
using EnrichcousBackOffice.Models;

namespace EnrichcousBackOffice.ViewControler
{
    public class SaleLeadViewService
    {
        public static List<C_SalesLead_Status> SalesLeadStatuses()
        {
            using (WebDataModel db = new WebDataModel())
            {
                return db.C_SalesLead_Status.OrderBy(st => st.Order).ToList();
            }
        }
    }
}