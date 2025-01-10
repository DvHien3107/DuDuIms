using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Builders;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using System;

namespace Enrich.IMS.Infrastructure.Data.Implement.Builders
{
    public class CustomerTransactionBuilder : ICustomerTransactionBuilder
    {
        private readonly ICustomerRepository _reppository;
        private readonly EnrichContext _context;
        public CustomerTransactionBuilder(IConnectionFactory connectionFactory, EnrichContext context, ICustomerRepository reppository)
        {
            _context = context;
            _reppository = reppository;
        }
        public CustomerTransaction BuildForFree(Order order)
        {
            return new CustomerTransaction()
            {
                Id = Guid.NewGuid().ToString("N"),
                CreateBy = _context.UserFullName,
                OrdersCode = order.OrdersCode,
                Amount = order.GrandTotal ?? 0,
                CustomerCode = order.CustomerCode,
                PaymentMethod = EnumHelper.DisplayName(PaymentEnum.Message.Free),
                PaymentNote = EnumHelper.DisplayDescription(PaymentEnum.Message.Free),
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                PaymentStatus = PaymentEnum.Status.Success.ToString(),
                ResponseText = EnumHelper.DisplayDescription(PaymentEnum.Message.Free),
                MxMerchant_authMessage = EnumHelper.DisplayDescription(PaymentEnum.Message.Free)
            };
        }
        public CustomerTransaction BuildForCreditCard(Order order, CustomerCard card)
        {
            if (card == null) card = new CustomerCard();
            return new CustomerTransaction()
            {
                Id = Guid.NewGuid().ToString("N"),
                CreateBy = _context.UserFullName,
                OrdersCode = order.OrdersCode,
                PaymentMethod = EnumHelper.DisplayName(PaymentEnum.Message.R_CreditCard),
                PaymentNote = EnumHelper.DisplayDescription(PaymentEnum.Message.R_CreditCard),
                Amount = order.GrandTotal ?? 0,
                CustomerCode = order.CustomerCode,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                Card = card.Id,
                BankName = card.CardType,
                CardNumber = card.CardNumber,
            };
        }
        public CustomerTransaction BuildForACH(Order order, Customer customer)
        {
            return new CustomerTransaction()
            {
                Id = Guid.NewGuid().ToString("N"),
                CreateBy = _context.UserFullName,
                OrdersCode = order.OrdersCode,
                PaymentMethod = EnumHelper.DisplayName(PaymentEnum.Message.R_ACH),
                PaymentNote = EnumHelper.DisplayDescription(PaymentEnum.Message.R_ACH),
                Amount = order.GrandTotal ?? 0,
                CustomerCode = order.CustomerCode,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                BankName = customer.DepositBankName,
                CardNumber = customer.DepositAccountNumber,
            };
        }
    }
}
