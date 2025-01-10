using Enrich.Common.Enums;
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
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public class RecurringHistoryService : GenericService<RecurringHistory, RecurringHistoryDto>, IRecurringHistoryService
    {
        private readonly IRecurringHistoryRepository _repository;
        private readonly IStoreServiceRepository _repositoryStoreService;
        private readonly ICustomerCardRepository _repositoryCustomerCard;
        private readonly IOrderSubscriptionBuilder _builderOrderSubscription;
        private readonly IOrderService _serviceOrder;
        private readonly IOrderSubscriptionService _serviceOrderSubscription;
        private readonly ICustomerTransactionService _serviceCustomerTransaction;

        public RecurringHistoryService(IRecurringHistoryRepository repository, IOrderSubscriptionMapper mapper,
            ICustomerTransactionService serviceCustomerTransaction, IStoreServiceRepository repositoryStoreService, IOrderSubscriptionService serviceOrderSubscription, 
            IOrderService serviceOrder, IOrderSubscriptionBuilder builderOrderSubscription, ICustomerCardRepository repositoryCustomerCard)
            : base(repository, mapper)
        {
            _repository = repository;
            _serviceCustomerTransaction = serviceCustomerTransaction;
            _repositoryStoreService = repositoryStoreService;
            _serviceOrderSubscription = serviceOrderSubscription;
            _serviceOrder = serviceOrder;
            _builderOrderSubscription = builderOrderSubscription;
            _repositoryCustomerCard = repositoryCustomerCard;
        }
    }
}
