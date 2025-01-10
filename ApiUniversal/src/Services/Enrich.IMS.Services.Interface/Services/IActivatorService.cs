using Enrich.Core.Infrastructure.Services;
using Enrich.Dto.Base.POSApi;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface IActivatorService //: IGenericService<OrderSubscription, OrderSubscriptionDto>
    {
        public Task ActivatorAction(string storeServiceId, string action, string mode = "");
    }
}
