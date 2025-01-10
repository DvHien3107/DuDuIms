using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Builders;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Builders
{
    public class OrderSubscriptionBuilder : IOrderSubscriptionBuilder
    {
        private readonly IOrderSubscriptionRepository _repository;
        private readonly EnrichContext _context;
        public OrderSubscriptionBuilder(IConnectionFactory connectionFactory, EnrichContext context, IOrderSubscriptionRepository repository)
        {
            _context = context;
            _repository = repository;
        }
        public OrderSubscription Build(OrderSubscription orderSubscription, string orderCode, Random random)
        {
            orderSubscription.Id = long.Parse(DateTime.UtcNow.ToString(Constants.Format.Date_yyMMddhhmmssff) + random.Next(1, 9999).ToString().PadLeft(4, '0'));
            orderSubscription.OrderCode = orderCode;
            orderSubscription.EndDate = _repository.CalculatorEndDate(orderSubscription);
            orderSubscription.PurcharsedDay = null;
            return orderSubscription;
        }

        public OrderSubscription Build(OrderSubscription orderSubscription, RecurringPlanning recurringPlanning)
        {
            if (orderSubscription == null) orderSubscription = new OrderSubscription();
            orderSubscription.Price = recurringPlanning.Price;
            orderSubscription.Discount = recurringPlanning.ApplyDiscountAsRecurring == true ? recurringPlanning.Discount : decimal.Zero;
            orderSubscription.DiscountPercent = recurringPlanning.ApplyDiscountAsRecurring == true ? recurringPlanning.DiscountPercent : decimal.Zero;
            orderSubscription.ApplyDiscountAsRecurring = recurringPlanning.ApplyDiscountAsRecurring;
            orderSubscription.Quantity = recurringPlanning.SubscriptionQuantity;
			orderSubscription.SubscriptionQuantity = recurringPlanning.SubscriptionQuantity;
            orderSubscription.StartDate = recurringPlanning.RecurringDate;
            orderSubscription.Amount = orderSubscription.Price * orderSubscription.SubscriptionQuantity - orderSubscription.Discount;
            return orderSubscription;
        }

    }
}
