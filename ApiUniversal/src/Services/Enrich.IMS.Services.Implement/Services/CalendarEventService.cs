using Enrich.Common;
using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Services.Implement;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public class CalendarEventService : GenericService<CalendarEvent, CalendarEventDto>, ICalendarEventService
    {
        private readonly ICalendarEventRepository _repository;
        private readonly EnrichContext _context;
        public CalendarEventService(ICalendarEventRepository repository, ICalendarEventMapper mapper, EnrichContext context)
            : base(repository, mapper)
        {
            _repository = repository;
            _context = context;
        }
    }
}
