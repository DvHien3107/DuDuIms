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
    public class StoreServiceBuilder : IStoreServiceBuilder
    {
        private readonly IStoreServiceRepository _repository;
        private readonly EnrichContext _context;
        public StoreServiceBuilder(IConnectionFactory connectionFactory, EnrichContext context)
        {
            _context = context;
        }
        public StoreService Build(StoreService storeService, OrderSubscription orderSubscription)
        {
            storeService.Id = Guid.NewGuid().ToString();
            storeService.RenewDate = orderSubscription.EndDate;
            storeService.OrderCode = orderSubscription.OrderCode;
            storeService.Active = -1;
            storeService.LastUpdateAt = DateTime.UtcNow;
            storeService.LastUpdateBy = _context.UserFullName;
            storeService.LastRenewAt = null;
            storeService.LastRenewBy = string.Empty;
            storeService.LastRenewOrderCode = string.Empty;
            storeService.HasRenewInvoiceIncomplete = false;
            return storeService;
        }

    }
}
