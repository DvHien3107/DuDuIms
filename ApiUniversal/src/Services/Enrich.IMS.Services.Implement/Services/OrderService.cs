using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Core;
using Enrich.Core.Container;
using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Builders;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Services.Implement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Enrich.IMS.Services.Implement.Services
{
    public class OrderService : GenericService<Order, OrderDto>, IOrderService
    {
        private readonly IOrderRepository _repository;
        private readonly ICustomerRepository _repositoryCustomer;
        private readonly ISalesLeadRepository _repositorySalesLead;
        private readonly IStoreServiceRepository _repositoryStoreService;
        private readonly ICalendarEventRepository _repositoryCalendarEvent;
        private readonly IOrderSubscriptionRepository _repositoryOdSubscription;
        private readonly EnrichContext _context;
        private readonly IEnrichLog _enrichLog;
        private readonly IEnrichContainer _container;
        private readonly IOrderBuilder _builder;
        private readonly IStoreServiceBuilder _buildertoreService;
        private readonly IOrderSubscriptionBuilder _builderOrderSubscription;
        private readonly Random _random;

        public OrderService(
            IEnrichLog enrichLog,
            IOrderRepository repository,
            ICustomerMapper mapper,
            EnrichContext context,
            ISalesLeadRepository repositorySalesLead,
            ICalendarEventRepository repositoryCalendarEvent,
            IEnrichContainer container,
            IOrderSubscriptionRepository repositoryOdSubscription,
            ICustomerRepository repositoryCustomer,
            IStoreServiceRepository repositoryStoreService,
            IOrderSubscriptionBuilder builderOrderSubscription,
            IOrderBuilder builder,
            IStoreServiceBuilder buildertoreService)
            : base(repository, mapper)
        {
            _enrichLog = enrichLog;
            _repository = repository;
            _context = context;
            _repositorySalesLead = repositorySalesLead;
            _repositoryCalendarEvent = repositoryCalendarEvent;
            _repositoryOdSubscription = repositoryOdSubscription;
            _repositoryCustomer = repositoryCustomer;
            _container = container;
            _repositoryStoreService = repositoryStoreService;
            _builderOrderSubscription = builderOrderSubscription;
            _builder = builder;
            _buildertoreService = buildertoreService;
            _random = new Random();
        }
        public async Task<Order> GetByOrderCodeAsync(string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode)) return new Order();
            return await _repository.GetByOrderCodeAsync(orderCode);
        }
        public async Task<Order> CreateAsync(Order _order, List<OrderSubscription> _ordersubscriptions)
        {
            if (_order == null) throw new Exception(ValidationMessages.Invalid.Order);
            if (_ordersubscriptions == null) throw new Exception(ValidationMessages.Invalid.OrderSubscription);

            //create order
            var order = _builder.Build(_order.CustomerCode, _ordersubscriptions);
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _repository.AddAsync(order);
                foreach (var os in _ordersubscriptions)
                {
                    //Update store service renew order and get to create new
                    var storeService = await _repositoryStoreService.UpdateGetRecurringAsync(os.OrderCode, order.OrdersCode, os.ProductCode);
                    //create order subscription
                    var newOSubscription = _builderOrderSubscription.Build(os, order.OrdersCode, _random);
                    await _repositoryOdSubscription.AddAsync(newOSubscription);
                    //create store service
                    var newStoreService = _buildertoreService.Build(storeService, newOSubscription);
                    await _repositoryStoreService.AddAsync(newStoreService);
                }
                transactionScope.Complete();
            }

            //Add calander event sale lead *not important
            //Push graylog if ERROR
            try
            {
                var salelead = _repositorySalesLead.GetByCustomerCode(order.CustomerCode);
                if (salelead == null) throw new Exception("Saleslead not found");
                string titleLog = $"Renew Invoice <b>#{order.OrdersCode}</b> has automation created.";
                string descriptionLog = $"Invoice: <a onclick='show_invoice(" + order.OrdersCode + ")'>#" + order.OrdersCode + "</a><br/>" +
                                    $"Create at: <span class='UTC-LOCAL'>" + DateTime.UtcNow.ToString(Constants.Format.Date_MMMddyyyy_HHmm) + "<span> <br/>";
                await _repositoryCalendarEvent.CreateAsync(salelead, order, titleLog, descriptionLog);
            }
            catch (Exception ex)
            {
                _enrichLog.Error(ex, $"Error add log calendar event create order {order.OrdersCode} customer {order.CustomerCode}");
            }

            //Check grand total = $0 => auto complete
            //Push graylog if ERROR
            try
            {
                if (order.GrandTotal == 0)
                {
                    var _transaction = _container.Resolve<ICustomerTransactionService>();
                    var transaction = await _transaction.CreateWithFree(order);
                    if (transaction.PaymentStatus == PaymentEnum.Status.Success.ToString())
                    {
                        order.Status = OrderEnum.Status.Paid_Wait.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _enrichLog.Error(ex, $"Error auto success order 0$ order {order.OrdersCode} customer {order.CustomerCode}");
            }

            return order;
        }
        public async Task<bool> ChangeStatusAsync(string orderId, string status)
        {
            var order = await _repository.FindByIdAsync(orderId);
            if (order == null) throw new Exception(ValidationMessages.NotFound.Order);
            if (!EnumHelper.IsDefined<OrderEnum.Status>(status)) throw new Exception(ValidationMessages.NotDefined.OrderStatus);
            if (status == order.Status) return true;

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = _context.UserFullName;
            order.UpdatedHistory += $"|{order.UpdatedAt?.ToString(Constants.Format.Date_MMMddyyyy_HHmm)} - By: {order.UpdatedBy}";
            order.StatusHistory += $"|{EnumHelper.DisplayName(EnumHelper.GetEnum<OrderEnum.Status>(status))} - Update by: {order.UpdatedBy} - At: {order.UpdatedAt?.ToString(Constants.Format.Date_MMMddyyyy_HHmm)}";
            await _repository.UpdateAsync(order);

            //Add calander event sale lead *not important
            //Push graylog if ERROR
            try
            {
                var salelead = await _repositorySalesLead.GetByCustomerCodeAsync(order.CustomerCode);
                //if (salelead == null) throw new Exception("Saleslead not found");
                string titleLog = string.Empty;
                string descriptionLog = string.Empty;
                if (order.Status != OrderEnum.Status.Paid_Wait.ToString() && order.Status != OrderEnum.Status.Closed.ToString())
                {
                    titleLog = $"Invoice <b>#{order.OrdersCode}</b> has been updated";
                    descriptionLog = $"Invoice <a onclick='show_invoice({order.OrdersCode})'> #{order.OrdersCode}</a> has been updated";
                }
                else if (order.Status == OrderEnum.Status.Closed.ToString())
                {
                    titleLog = "Invoice <b>#" + order.OrdersCode + "</b> has been completed";
                    descriptionLog = "Invoice <a onclick='show_invoice(" + order.OrdersCode + ")'> #" + order.OrdersCode + "</a> has been completed";
                }
                await _repositoryCalendarEvent.CreateAsync(salelead, order, titleLog, descriptionLog);
            }
            catch (Exception)
            {
                //add Graylog
            }

            return true;
        }
    }
}
