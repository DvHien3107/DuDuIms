using Enrich.Common.Enums;
using Enrich.IMS.Dto.Customer;
using Enrich.IMS.Dto.Subscription;
using Pos.Application.Services.Scoped;
using Pos.Model.Model.Table.IMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Event
{
    public interface ICustomerEvent
    {
        Task<CustomerSearchResponse> SearchAsyncv2(CustomerSearchRequest request);
        Task<C_Customer> getCustomerByCode(string CustomerCode);
        Task updateCustomerMerchantId(string MxMerchant_Id, string CustomerCode);
    }

    public class CustomerEvent : ICustomerEvent
    {
        private readonly ICustomerService _cusService;

        public CustomerEvent( ICustomerService cusService)
        {
            _cusService = cusService;
        }
        public async Task updateCustomerMerchantId(string MxMerchant_Id, string CustomerCode)
        {
            await _cusService.updateCustomerMerchantId(MxMerchant_Id, CustomerCode);
        }
        public async Task<C_Customer> getCustomerByCode(string CustomerCode)
        {
            return await _cusService.getCustomerInfoByCode(CustomerCode);
        }

        public async Task<CustomerSearchResponse> SearchAsyncv2(CustomerSearchRequest request)
        {
            var response = await _cusService.SearchAsyncv2(request);
            //await PopulateItems(response);
            var listStoreCode = response.Records.Select(x => x.StoreCode).Where(c => !string.IsNullOrEmpty(c));
            var listLicense = await _cusService.GetLicenseStatusAsync(listStoreCode);

            foreach (var customer in response.Records)
            {
                var license = listLicense.FirstOrDefault(x => x.StoreCode == customer.StoreCode);
                if (license != null)
                {
                    customer.License = new LicenseStatusDto()
                    {
                        LicenseName = license.LicenseName,
                        RemainingDate = license.RemainingDate,
                        EffectiveDate = license.EffectiveDate,
                        LicenseStatus = (int)SubscriptionEnum.LicenseStatus.Active
                    };
                    if (license.RemainingDate <= 0)
                    {
                        customer.License.LicenseStatus = (int)SubscriptionEnum.LicenseStatus.Expired;
                    }
                    if (license.EffectiveDate.Date > System.DateTime.UtcNow.Date)
                    {
                        customer.License.LicenseStatus = (int)SubscriptionEnum.LicenseStatus.FutureActive;
                    }
                }
                if (!string.IsNullOrEmpty(customer.MID))
                {
                    customer.Terminal = new TerminalStatusDto()
                    {
                        MID = customer.MID,
                        Status = customer.TerminalStatus == 1 ? (int)SubscriptionEnum.TerminalStatus.Active : (int)SubscriptionEnum.TerminalStatus.InActive,
                        TerminalType = customer.TerminalType
                    };

                }

            }
            return response;
        }

        
    }
}
