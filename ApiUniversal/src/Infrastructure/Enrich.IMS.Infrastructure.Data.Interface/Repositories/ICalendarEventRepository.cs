using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface ICalendarEventRepository : IGenericRepository<CalendarEvent>
    {
        public Task<CalendarEvent> CreateAsync(CalendarEvent calendarEvent);
        public Task<CalendarEvent> CreateAsync(SalesLead salelead, Order order, string title = "", string description = "");
        public Task<CalendarEvent> CreateAsync(StoreService storeService, string title = "", string description = "");
    }
}

