using Dapper;
using Enrich.Common;
using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public class CalendarEventRepository : DapperGenericRepository<CalendarEvent>, ICalendarEventRepository
    {
        private readonly EnrichContext _context;
        public CalendarEventRepository(IConnectionFactory connectionFactory, EnrichContext context)
            : base(connectionFactory)
        {
            _context = context;
        }
        public async Task<CalendarEvent> CreateAsync(CalendarEvent calendarevent)
        {
            if (calendarevent == null) calendarevent = new CalendarEvent();
            calendarevent.Id = Guid.NewGuid().ToString();
            calendarevent.CreateAt = calendarevent.CreateAt ?? DateTime.UtcNow;
            calendarevent.CreateBy = calendarevent.CreateBy ?? _context.UserFullName;
            calendarevent.LastUpdateBy = calendarevent.CreateBy;
            calendarevent.LastUpdateAt = calendarevent.CreateAt;
            calendarevent.GMT = "+00:00";
            calendarevent.TimeZone = "(UTC) Coordinated Universal Time";
            calendarevent.StartEvent = DateTime.UtcNow.ToString(Constants.Format.Date_MMddyyyy_HHmmss) + calendarevent.GMT;
            calendarevent.Done = 1;
            await AddAsync(calendarevent);
            return calendarevent;
        }
        public async Task<CalendarEvent> CreateAsync(SalesLead salelead, Order order, string title = "", string description = "")
        {
            var calender_event = new CalendarEvent();
            calender_event.CustomerCode = order.CustomerCode;
            calender_event.CustomerName = order.CustomerName;
            calender_event.SalesLeadId = salelead.Id;
            calender_event.SalesLeadName = salelead.CustomerName;
            calender_event.Name = title;
            calender_event.Description = description;
            var calendarevent = await CreateAsync(calender_event);
            return calendarevent;
        }
        public async Task<CalendarEvent> CreateAsync(StoreService storeService, string title = "", string description = "")
        {
            var saleLead = new SalesLead();
            var query = $"SELECT TOP 1 * FROM {SqlTables.SalesLead} WITH (NOLOCK) WHERE [CustomerCode] = @customercode";
            var parameters = new DynamicParameters();
            parameters.Add("customercode", storeService.CustomerCode);
            using (var connection = GetDbConnection())
            {
                saleLead = connection.QueryFirstOrDefault<SalesLead>(query, parameters);
            }
            var calender_event = new CalendarEvent();
            calender_event.CustomerCode = storeService.CustomerCode;
            calender_event.CustomerName = storeService.StoreName;
            calender_event.SalesLeadId = saleLead.Id;
            calender_event.SalesLeadName = saleLead.CustomerName;
            calender_event.Name = title;
            calender_event.Description = description;
            var calendarevent = await CreateAsync(calender_event);
            return calendarevent;
        }
    }
}