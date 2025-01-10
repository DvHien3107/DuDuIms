using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Builders;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Builders
{
    public class BusinessEventBuilder : IBusinessEventBuilder
    {
        private readonly EnrichContext _context;
        private readonly ISystemConfigurationRepository _repositorySystem;

        public BusinessEventBuilder(
            EnrichContext context,
            ISystemConfigurationRepository repositorySystem)
        {
            _context = context;
            _repositorySystem = repositorySystem;
        }

        /// <summary>
        /// Builder data for create new business event
        /// </summary>
        /// <param name="businessEvent"></param>
        public void BuildForSave(BusinessEvent businessEvent)
        {
            businessEvent.CreateAt = TimeHelper.GetUTCNow();
            businessEvent.CreateBy = _context.UserFullName;
        }
    }
}