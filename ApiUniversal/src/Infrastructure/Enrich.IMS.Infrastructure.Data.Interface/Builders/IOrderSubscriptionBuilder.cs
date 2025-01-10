using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Builders
{
    public interface IOrderSubscriptionBuilder
    {
        public OrderSubscription Build(OrderSubscription orderSubscription, string orderCode, Random random);
        public OrderSubscription Build(OrderSubscription orderSubscription, RecurringPlanning recurringPlanning);
    }
}
