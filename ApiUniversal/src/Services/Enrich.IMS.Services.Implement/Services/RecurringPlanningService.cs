using Enrich.Common.Enums;
using Enrich.Core;
using Enrich.Core.Container;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Builders;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Implement.Services.NextGen.IBO;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Services.Implement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public class RecurringPlanningService : GenericService<RecurringPlanning, RecurringPlanningDto>, IRecurringPlanningService
    {
        private readonly IRecurringPlanningRepository _repository;
        private readonly IStoreServiceRepository _repositoryStoreService;
        private readonly ICustomerCardRepository _repositoryCustomerCard;
        private readonly IOrderSubscriptionBuilder _builderOrderSubscription;
        private readonly IOrderService _serviceOrder;
        private readonly IOrderSubscriptionService _serviceOrderSubscription;
        private readonly ICustomerTransactionService _serviceCustomerTransaction;
        private readonly IRecurringHistoryRepository _repositoryRecurringHistory;
        private readonly ICustomerRepository _repositoryCustomer;
        private readonly IRecurringHistoryBuilder _builderRecurringHistory;
        private readonly IEnrichLog _enrichLog;
        private readonly IIboSyncService _iboSync;

        public RecurringPlanningService(
            IRecurringPlanningRepository repository,
            IOrderSubscriptionMapper mapper,
            IStoreServiceRepository repositoryStoreService,
            IOrderSubscriptionBuilder builderOrderSubscription,
            ICustomerCardRepository repositoryCustomerCard,
            IRecurringHistoryBuilder builderRecurringHistory,
            IRecurringHistoryRepository repositoryRecurringHistory,
            IEnrichLog enrichLog,
            IEnrichContainer _container,
            ICustomerRepository repositoryCustomer,
            IIboSyncService iboSync
            )
            : base(repository, mapper)
        {
            _repository = repository;
            _iboSync = iboSync;
            _repositoryStoreService = repositoryStoreService;
            _builderOrderSubscription = builderOrderSubscription;
            _repositoryCustomerCard = repositoryCustomerCard;
            _builderRecurringHistory = builderRecurringHistory;
            _repositoryRecurringHistory = repositoryRecurringHistory;
            _enrichLog = enrichLog;
            _serviceCustomerTransaction = _container.Resolve<ICustomerTransactionService>();
            _serviceOrderSubscription = _container.Resolve<IOrderSubscriptionService>();
            _serviceOrder = _container.Resolve<IOrderService>(); ;
            _repositoryCustomer = repositoryCustomer;
        }
        public async Task<IEnumerable<RecurringPlanning>> GetByRecurringDate()
        {
            return await _repository.GetByRecurringDate();
        }

        public async Task<Order> RecurringSubsciptionActionAsync(List<RecurringPlanning> recurringPlannings)
        {
            var sessionId = Guid.NewGuid().ToString();
            string message = string.Empty;
            if (recurringPlannings == null || recurringPlannings.Count() == 0) throw new Exception(ValidationMessages.Invalid.RecurringPlanning);
            var subscriptions = new List<OrderSubscription>();
            foreach (var plan in recurringPlannings)
            {
                var subscription = _serviceOrderSubscription.GetByOrderCodeNSubscriptionCode(plan.OrderCode, plan.SubscriptionCode);
                subscription = _builderOrderSubscription.Build(subscription, plan);
                subscriptions.Add(subscription);
            }
            var customerCode = recurringPlannings.FirstOrDefault().CustomerCode;
            _enrichLog.Info($"[{sessionId}] [{customerCode}] Start recurring");
            //create order + order detail
            var order = new Order { CustomerCode = customerCode };
            var _result = await _serviceOrder.CreateAsync(order, subscriptions);

            if (_result != null)// create order success
            {
                _enrichLog.Info($"[{sessionId}] [{customerCode}] Create recurring order {_result.OrdersCode} customer {_result.CustomerCode}",new { Customer= order.CustomerCode});
                message += $"{RecurringEnum.Message.CreateOrder} {_result.OrdersCode}";

                //payment
                if (_result.Status != OrderEnum.Status.Paid_Wait.ToString() && _result.Status != OrderEnum.Status.Closed.ToString())
                {
                    try
                    {
                        var transaction = new CustomerTransaction();
                        if (recurringPlannings.FirstOrDefault().RecurringType?.ToLower() == RecurringEnum.Type.ACH.ToString().ToLower())
                        {
                            message += $"|{RecurringEnum.Message.PaidWACH}";
                            transaction = await _serviceCustomerTransaction.CreateWithACH(_result);
                            if (transaction.PaymentStatus == PaymentEnum.Status.Approved.ToString()) _result.Status = OrderEnum.Status.Paid_Wait.ToString();
                        }
                        else
                        {
                            var storeService = _repositoryStoreService.GetByOrderCodeNSubscriptionCode(_result.OrdersCode, recurringPlannings.FirstOrDefault().SubscriptionCode);
                            var card = _repositoryCustomerCard.GetForRecurring(storeService.MxMerchantCardAccountId, customerCode);

                            if (card == null)
                            {
                                var conditionExistACH = _repositoryCustomer.IsExistACH(order.CustomerCode);
                                if (conditionExistACH) //exist ACH => paymet by ACH
                                {
                                    message += $"|{RecurringEnum.Message.PaidWACH}";
                                    transaction = await _serviceCustomerTransaction.CreateWithACH(_result);
                                    if (transaction.PaymentStatus == PaymentEnum.Status.Approved.ToString()) _result.Status = OrderEnum.Status.Paid_Wait.ToString();
                                }
                                else //not yet payment
                                {
                                    message += $"|{RecurringEnum.Message.NotYet}";
                                    await _serviceCustomerTransaction.ActionPaymentFailed(_result, null, null);
                                }
                                goto UPDATEPLANNING;
                            }

                            message += $"|{RecurringEnum.Message.PaidWCard} {card?.CardNumber}";
                            transaction = await _serviceCustomerTransaction.CreateWithCreditCard(_result, card);
                            if (transaction.PaymentStatus == PaymentEnum.Status.Approved.ToString()) 
                                _result.Status = OrderEnum.Status.Paid_Wait.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        _enrichLog.Error(ex, $"[{sessionId}] Error payment recurring order {_result.OrdersCode}  customer {_result.CustomerCode}");
                        message += $"|Payment error";
                    }
                }
                else message += $"|{RecurringEnum.Message.PaidZero}";
            }
            else message += $"{RecurringEnum.Message.CreateOrder} error";

            UPDATEPLANNING:
            foreach (var plan in recurringPlannings)
            {
                try
                {
                    //save history
                    var recurringHistory = _builderRecurringHistory.Build(plan, _result, message);
                    await _repositoryRecurringHistory.AddAsync(recurringHistory);
                    _enrichLog.Info($"[{sessionId}] [{customerCode}] Create recurring history for planning {plan.Id} customer {_result.CustomerCode}");
                    //update for new plan
                    var subscription = _serviceOrderSubscription.GetByOrderCodeNSubscriptionCode(_result.OrdersCode, plan.SubscriptionCode);
                    plan.OrderCode = _result.OrdersCode;
                    plan.StartDate = subscription.StartDate;
                    plan.EndDate = subscription.EndDate;
                    plan.RecurringDate = subscription.EndDate;
                    await _repository.UpdateAsync(plan);
                    //await _clickUpConnectorService.SyncMerchantToClickUpAsync(_result.CustomerCode);
                    await _iboSync.CallHookSyncClickup(_result.CustomerCode);
                }
                catch (Exception ex) //update fail => stop recurring for this record
                {
                    _enrichLog.Error(ex, $"[{sessionId}] [{customerCode}] Error update recurring planning, planning {plan.Id} customer {plan.CustomerCode}");
                    //update for new plan
                    plan.Status = (int)RecurringEnum.PlanStatus.Disable;
                    await _repository.UpdateAsync(plan);
                }
            }

            _enrichLog.Info($"[{sessionId}] Complete recurring customer {customerCode}");
            return _result;
        }

        public async Task RecurringSubscriptionAsync()
        {
            var recurringPlans = await GetByRecurringDate();
            var recurringPlansGroupbyCustomer = recurringPlans.GroupBy(c => new { c.CustomerCode, c.RecurringType }).AsEnumerable();
            _enrichLog.Info($"Start recurring for {recurringPlansGroupbyCustomer?.Count()} customers with {recurringPlans?.Count()} subscriptions");
         
            foreach (var item in recurringPlansGroupbyCustomer)
            {
                try
                {
                    await RecurringSubsciptionActionAsync(item.ToList());
                }
                catch (Exception ex)
                {
                    _enrichLog.Error(ex, "Error recurring " + item.First().CustomerCode);
                }
            }
            _enrichLog.Info($"End recurring for {recurringPlansGroupbyCustomer?.Count()} customer with {recurringPlans?.Count()} subscriptions");
        }
        public async Task DoProc(string query)
        {
            await _serviceOrderSubscription.DoProc(query);
        }

        public async Task<string> RecurringSubscriptionAsync_NextGen()
        {
            var recurringPlans = await GetByRecurringDate();
            var recurringPlansGroupbyCustomer = recurringPlans.GroupBy(c => new { c.CustomerCode, c.RecurringType }).AsEnumerable();
            _enrichLog.Info($"Start recurring for {recurringPlansGroupbyCustomer?.Count()} customers with {recurringPlans?.Count()} subscriptions");
            string result = "";
            result += $"\r\nStart recurring for {recurringPlansGroupbyCustomer?.Count()} customers with {recurringPlans?.Count()} subscriptions";
            foreach (var item in recurringPlansGroupbyCustomer)
            {
                try
                {
                    await RecurringSubsciptionActionAsync(item.ToList());
                }
                catch (Exception ex)
                {
                    _enrichLog.Error(ex, "Error recurring " + item.First().CustomerCode);
                    result += "\r\n -- Error recurring " + item.First().CustomerCode+":" + ex.ToString();
                }
            }
            _enrichLog.Info($"End recurring for {recurringPlansGroupbyCustomer?.Count()} customer with {recurringPlans?.Count()} subscriptions");
            result += $"\r\nEnd recurring for {recurringPlansGroupbyCustomer?.Count()} customer with {recurringPlans?.Count()} subscriptions";
            return result;
        }

        public async Task RecurringSubscriptionByCustomerCodeAsync(string customerCode)
        {
			var recurringPlans = await GetByRecurringDate();
			var recurringPlansGroupbyCustomer = recurringPlans.GroupBy(c => new { c.CustomerCode, c.RecurringType }).AsEnumerable().FirstOrDefault(x=>x.FirstOrDefault().CustomerCode == customerCode);
			_enrichLog.Info($"Start recurring for {recurringPlansGroupbyCustomer?.Count()} customers with {recurringPlans?.Count()} subscriptions");
		
            try
			{
				await RecurringSubsciptionActionAsync(recurringPlansGroupbyCustomer.ToList());
			}
			catch (Exception ex)
			{
				_enrichLog.Error(ex, "Error recurring " + recurringPlansGroupbyCustomer.First().CustomerCode);
			}

			_enrichLog.Info($"End recurring for {recurringPlansGroupbyCustomer?.Count()} customer with {recurringPlans?.Count()} subscriptions");
		}
	}
}
