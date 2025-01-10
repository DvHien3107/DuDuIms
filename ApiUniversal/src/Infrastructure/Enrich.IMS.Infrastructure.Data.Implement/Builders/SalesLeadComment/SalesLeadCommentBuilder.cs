using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Builders;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using System;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Builders
{
    public class SalesLeadCommentBuilder : ISalesLeadCommentBuilder
    {
        private readonly EnrichContext _context;
        private readonly ISalesLeadCommentRepository _repository;
        private readonly ISystemConfigurationRepository _repositorySystem;

        public SalesLeadCommentBuilder(
            EnrichContext context,
            ISalesLeadCommentRepository repository,
            ISystemConfigurationRepository repositorySystem)
        {
            _context = context;
            _repository = repository;
            _repositorySystem = repositorySystem;
        }

        /// <summary>
        /// Builder data for create new Sales Lead
        /// </summary>
        /// <param name="salesLead">Sales lead data dto</param>
        public async Task BuildForSave(bool isNew, SalesLeadCommentDto salesLeadComment)
        {
            if (isNew)
            {
                salesLeadComment.CreateAt = TimeHelper.GetUTCNow();
                salesLeadComment.CreateBy = _context.UserNumber;
            }
            salesLeadComment.UpdateAt = TimeHelper.GetUTCNow();
            salesLeadComment.UpdateBy = _context.UserNumber;
        }
    }
}