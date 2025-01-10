using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Builders
{
    public interface IRecurringHistoryBuilder
    {
        public RecurringHistory Build(RecurringPlanning recurringPlanning, Order order, string message = "");
    }
}
