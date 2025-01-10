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
    public class SalesLeadBuilder : ISalesLeadBuilder
    {
        private readonly EnrichContext _context;
        private readonly ISalesLeadRepository _repository;
        private readonly ISystemConfigurationRepository _repositorySystem;

        public SalesLeadBuilder(
            EnrichContext context,
            ISalesLeadRepository repository,
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
        public async Task BuildForSave(bool isNew, SalesLeadDto salesLead)
        {
            salesLead.CustomerName = salesLead.SalonName;
            salesLead.UpdateAt = TimeHelper.GetUTCNow();
            salesLead.UpdateBy = _context.UserFullName;
            salesLead.CreateByMemberNumber = _context.UserNumber;

            if (!string.IsNullOrEmpty(salesLead.LastNote))
                salesLead.LastNoteAt = TimeHelper.GetUTCNow();
            
            if (isNew)
            {
                var systemConfig = await _repositorySystem.GetSystemConfigurationAsync() ?? new SystemConfiguration();
                salesLead.Id = Guid.NewGuid().ToString();
                salesLead.CustomerCode = await _repository.GenerateCustomerCode();
                salesLead.SalonAddress = StringHelper.RemoveSpecialCharacter(salesLead.SalonAddress);
                salesLead.CreateAt = TimeHelper.GetUTCNow();
                salesLead.CreateBy = _context.UserFullName;
                salesLead.Password = systemConfig.MerchantPasswordDefault ?? string.Empty;
                if (!string.IsNullOrEmpty(salesLead.Version)) // dont use any more
                {
                    salesLead.CreateTrialBy = _context.UserFullName;
                    salesLead.CreateTrialAt = TimeHelper.GetUTCNow();
                }
            }
        }

        public async Task BuildForImport(SalesLeadDto salesLead)
        {
            var systemConfig = await _repositorySystem.GetSystemConfigurationAsync() ?? new SystemConfiguration();
            salesLead.Id = Guid.NewGuid().ToString();
            salesLead.SalonAddress = StringHelper.RemoveSpecialCharacter(salesLead.SalonAddress);
            salesLead.CreateAt = DateTime.UtcNow; //TimeHelper.GetUTCNow();
            salesLead.CreateBy = _context.UserFullName;
            salesLead.Password = systemConfig.MerchantPasswordDefault ?? string.Empty;
            salesLead.Type = EnumHelper.DisplayName(SalesLeadEnum.Type.ImportData);
            salesLead.CreateByMemberNumber = _context.UserNumber;
            salesLead.IsVerify = false;
            salesLead.IsSendMail = false;
            salesLead.MemberNumber = _context.UserNumber;
        }
    }
}