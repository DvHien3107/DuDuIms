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

namespace Enrich.IMS.Infrastructure.Data.Implement.Builders
{
    public class OrderBuilder : IOrderBuilder
    {
        private readonly IOrderRepository _repository;
        private readonly ICustomerRepository _reppositoryCustomer;
        private readonly EnrichContext _context;
        public OrderBuilder(IConnectionFactory connectionFactory, EnrichContext context, ICustomerRepository reppositoryCustomer, IOrderRepository repository)
        {
            _context = context;
            _repository = repository;
            _reppositoryCustomer = reppositoryCustomer;
        }
        public Order Build(string customerCode, List<OrderSubscription> ordersubscriptions)
        {
            if (string.IsNullOrEmpty(customerCode)) throw new Exception(ValidationMessages.Invalid.Customer);
            var customer = _reppositoryCustomer.GetByCustomerCode(customerCode);
            if (customer == null) throw new Exception(ValidationMessages.Invalid.Customer);

            var order = new Order();
            if (order == null) order = new Order();
            order.Id = long.Parse(DateTime.UtcNow.ToString(Constants.Format.Date_yyMMddhhmmssfff));
            var genarateCode = _repository.GetNumberOrderOnDate() + 1;
            order.OrdersCode = DateTime.UtcNow.ToString(Constants.Format.Date_yyMM) + genarateCode.ToString().PadLeft(4, '0') + DateTime.UtcNow.ToString(Constants.Format.Date_fff);
            order.ShippingFee = 0;
            order.DiscountAmount = 0;
            order.DiscountPercent = 0;
            order.TaxRate = 0;
            order.TotalHardwareAmount = 0;
            order.CreatedAt = DateTime.UtcNow;
            order.CreatedBy = _context.UserFullName ?? Constants.SystemName.ToString();
            order.CustomerCode = customer.CustomerCode;
            order.CustomerName = customer.BusinessName;
            order.PartnerCode = customer.PartnerCode;
            order.Status = OrderEnum.Status.Open.ToString();
            order.InvoiceDate = DateTime.UtcNow.Date;
            order.InvoiceNumber = ConvertHelper.GetLong(order.OrdersCode);
            order.BundelStatus = OrderEnum.DEPLOYMENT_PACKAGE_STATUS.Preparation.ToString();
            order.GrandTotal = CalculatorGrandTotal(ordersubscriptions);
            order.ServiceAmount = CalculatorGrandTotal(ordersubscriptions);
            order.UpdatedHistory = $"{order.CreatedAt?.ToString(Constants.Format.Date_MMMddyyyy_HHmm)} - By: {order.CreatedBy}";
            order.StatusHistory = $"{EnumHelper.DisplayName(OrderEnum.Status.Open)} - Create by: {order.CreatedBy} - At: {order.CreatedAt?.ToString(Constants.Format.Date_MMMddyyyy_HHmm)}";
            return order;
        }

        private decimal CalculatorGrandTotal(List<OrderSubscription> orderSubscriptions)
        {
            if (orderSubscriptions == null) return decimal.Zero;
            var total = orderSubscriptions.Sum(c => c.Price * (c.Period == SubscriptionEnum.Period.MONTHLY.ToString() ? (c.SubscriptionQuantity ?? 1) : (c.Quantity ?? 1))
                                                    - ((c.ApplyDiscountAsRecurring == true ? c.Discount : 0) ?? 0)) ?? decimal.Zero;
            return total;
        }
    }
}
