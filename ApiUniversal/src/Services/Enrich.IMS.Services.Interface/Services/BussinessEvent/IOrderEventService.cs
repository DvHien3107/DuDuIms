using Enrich.BusinessEvents.IMS;
using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface IOrderEventService : IGenericService<OrderEvent, OrderEventDto>, IBusinessEventService
    {

        /// <summary>
        /// Send notification payment later list to config emails
        /// </summary>
        /// <param name="events"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task SendNotificationPaymentLaterAsync(IEnumerable<BaseEvent<OrderPaymentLaterEvent>> events, string sessionId = "");


        /// <summary>
        /// Send notification payment failed list to config emails
        /// </summary>
        /// <param name="events"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task SendNotificationPaymentFailedAsync(IEnumerable<BaseEvent<OrderPaymentFailedEvent>> events, string sessionId = "");
    }
}