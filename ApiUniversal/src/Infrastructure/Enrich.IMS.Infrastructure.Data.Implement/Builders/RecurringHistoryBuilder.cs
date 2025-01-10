using Enrich.Common.Enums;
using Enrich.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Builders;
using Enrich.Infrastructure.Data;
using System;

namespace Enrich.IMS.Infrastructure.Data.Implement.Builders
{
    public class RecurringHistoryBuilder : IRecurringHistoryBuilder
    {
        private readonly EnrichContext _context;
        public RecurringHistoryBuilder(IConnectionFactory connectionFactory, EnrichContext context)
        {
            _context = context;
        }
        public RecurringHistory Build(RecurringPlanning recurringPlanning, Order order, string message = "")
        {
            return new RecurringHistory
            {
                RecurringId = recurringPlanning.Id,
                OldOrderCode = recurringPlanning.OrderCode,
                RecurringOrder = order.OrdersCode,
                Status = (order.Status == OrderEnum.Status.Paid_Wait.ToString() || order.Status == OrderEnum.Status.Closed.ToString()) ? (int)RecurringEnum.Status.Success : (int)RecurringEnum.Status.Failed,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = order.CreatedBy,
                TotalPrice = order.GrandTotal
            };
        }

    }
}
