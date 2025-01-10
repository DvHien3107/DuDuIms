using Dapper;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface;
using Enrich.IMS.Infrastructure.Data.Interface.Builders;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public partial class BusinessEventRepository : DapperGenericRepository<BusinessEvent>, IBusinessEventRepository
    {
        private readonly IBusinessEventBuilder _businessEventBuilder;
        public BusinessEventRepository(IConnectionFactory connectionFactory, IBusinessEventBuilder businessEventBuilder)
            : base(connectionFactory)
        {
            _businessEventBuilder = businessEventBuilder;
        }

        public async Task CreateAsync(string NameSpace, string Event, string Description, string MetaData)
        {
            var _event = new BusinessEvent
            {
                NameSpace = NameSpace,
                Event = Event,
                Description = Description,
                MetaData = MetaData
            };
            _businessEventBuilder.BuildForSave(_event);

            await AddAsync(_event);
        }
    }
}