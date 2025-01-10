using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Builders;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;

namespace Enrich.IMS.Infrastructure.Data.Implement.Builders
{
    public class CustomerBuilder : ICustomerBuilder
    {
        private readonly EnrichContext _context;
        private readonly ISystemConfigurationRepository _repositorySystem;

        public CustomerBuilder(
            EnrichContext context,
            ISystemConfigurationRepository repositorySystem)
        {
            _context = context;           
            _repositorySystem = repositorySystem;
        }

        /// <summary>
        /// Builder data for create new Customer. Using for create or update sale lead
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="salesLead"></param>
        public void BuildForSaveFromSalesLead(bool isnew, CustomerDto customer, SalesLeadDto salesLead)
        {
            if (customer == null)
                return;
            // to create new sale lead
            if (isnew)
            {
                var systemConfig = _repositorySystem.GetSystemConfiguration() ?? new SystemConfiguration();
                customer.Id = long.Parse(TimeHelper.GetUTCNow().ToString(Constants.Format.Date_yyMMddhhmmssfff));     
                customer.StoreCode = StringHelper.ReplaceSpecialChars(salesLead.CustomerCode, (SalesLeadEnum.SpecialCode.Customer, SalesLeadEnum.SpecialCode.Store));
                customer.CreateAt = TimeHelper.GetUTCNow();
                customer.CreateBy = _context.UserFullName;
                customer.Password = systemConfig.MerchantPasswordDefault ?? string.Empty;
                customer.MD5PassWord = SecurityHelper.Md5Encrypt(customer.Password);
                customer.Active = (int)CustomerEnum.StatusSearch.Active;
            }
            customer.CustomerCode = salesLead.CustomerCode;
            customer.BusinessEmail = customer.SalonEmail = customer.MangoEmail = customer.Email = salesLead.SalonEmail;
            customer.BusinessName = salesLead.SalonName;
            customer.OwnerName = customer.ContactName = salesLead.ContactName;
            customer.OwnerMobile = customer.CellPhone = salesLead.ContactPhone;
            customer.Country = customer.BusinessCountry = salesLead.Country;
            customer.SalonAddress1 = customer.BusinessAddressStreet = customer.BusinessAddress = customer.SalonAddress1 = salesLead.SalonAddress;
            customer.SalonCity = customer.BusinessCity = customer.City = salesLead.City;
            customer.SalonPhone = customer.BusinessPhone = salesLead.SalonPhone;
            customer.SalonState = customer.BusinessState = customer.State = salesLead.State;
            customer.SalonZipcode = customer.BusinessZipCode = customer.Zipcode = salesLead.Zipcode;
            customer.SalonTimeZone = salesLead.SalonTimeZone;
            customer.SalonTimeZoneNumber = salesLead.SalonTimeZoneNumber;
            customer.MoreInfo = salesLead.MoreInfo;

        }
    }
}
