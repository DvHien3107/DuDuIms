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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Builders
{
    public class NewCustomerGoalBuilder : INewCustomerGoalBuilder
    {
        private readonly INewCustomerGoalRepository _repository;
        private readonly EnrichContext _context;
        public NewCustomerGoalBuilder(IConnectionFactory connectionFactory, EnrichContext context)
        {
            _context = context;
        }
        public void BuildForSave(bool isNew, NewCustomerGoalDto newGoal)
        {
            if (isNew)
            {
                newGoal.CreatedDate = TimeHelper.GetUTCNow();
                newGoal.CreatedBy = _context.UserFullName;
                newGoal.SiteId = _context.SiteId;
            }
            else
            {
                newGoal.UpdatedDate = TimeHelper.GetUTCNow();
                newGoal.UpdatedBy = _context.UserFullName;
            }
        }

    }
}
