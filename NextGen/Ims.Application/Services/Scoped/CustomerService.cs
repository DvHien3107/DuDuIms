using Dapper;
using Enrich.Common.Enums;
using Enrich.Dto.List;
using Enrich.IMS.Dto.Customer;
using Enrich.IMS.Dto.Subscription;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Pos.Application.Extensions.Helper;
using Pos.Model.Model.Proc;
using Pos.Model.Model.Request;
using Pos.Model.Model.Table.IMS;
using Promotion.Application.Extensions;
using Promotion.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Pos.Application.Services.Scoped
{

    public interface ICustomerService : IEntityService<TwilioAccount>
    {
        Task<CustomerSearchResponse> SearchAsyncv2(CustomerSearchRequest request);
        Task<IEnumerable<LicenseStatusDto>> GetLicenseStatusAsync(IEnumerable<string> listStoreCode);
        Task<StoreLicenses> getCustomerInfoByCode(string CustomerCode, string OrderCode);
        Task<C_Customer> getCustomerInfoByCode(string CustomerCode);
        Task updateCustomerMerchantId(string MxMerchant_Id, string CustomerCode);
        Task updateCustomerPaymentProfileId(string PaymentProileId, string CustomerCode);
    }
    public class CustomerService : IMSEntityService<TwilioAccount>, ICustomerService
    {
        public async Task updateCustomerMerchantId(string MxMerchant_Id, string CustomerCode)
        {
            await _connection.ExecuteAsync("update C_Customer set MxMerchant_Id = @MxMerchant_Id where CustomerCode = @CustomerCode", new { MxMerchant_Id, CustomerCode });
        }
        public async Task updateCustomerPaymentProfileId(string PaymentProileId, string CustomerCode)
        {
            await _connection.ExecuteAsync("update C_Customer set DepositAccountNumber = @PaymentProileId where CustomerCode = @CustomerCode", new { PaymentProileId, CustomerCode });
        }

        public async Task<C_Customer> getCustomerInfoByCode(string CustomerCode)
        {
            return await _connection.SqlFirstOrDefaultAsync<C_Customer>("select * from C_Customer with (nolock) where CustomerCode = @CustomerCode", new { CustomerCode });
        }
        public async Task<IEnumerable<LicenseStatusDto>> GetLicenseStatusAsync(IEnumerable<string> listStoreCode)
        {

            int[] storeServiceStatus = { (int)SubscriptionEnum.Status.Active };
            var query = @$"select top 1000 
Store_Services.StoreCode,
Store_Services.RenewDate,
Store_Services.Productname as 'LicenseName',
Store_Services.[Active] as 'LicenseStatus',
CASE
    WHEN CONVERT(date,GETUTCDATE()) >= Store_Services.EffectiveDate THEN DATEDIFF(day,GETUTCDATE(),RenewDate)
    ELSE DATEDIFF(day,Store_Services.EffectiveDate ,RenewDate)
END AS 'RemainingDate',

Store_Services.EffectiveDate from Store_Services INNER JOIN License_Product ON Store_Services.ProductCode = License_Product.Code
where Store_Services.[Type] ='license' AND Store_Services.[Active] IN @storeServiceStatus
AND StoreCode IN @storeCodes
";
            var result = await _connection.QueryAsync<LicenseStatusDto>(query, new
            {
                storeCodes = listStoreCode,
                storeServiceStatus = storeServiceStatus
            });
            return result;
        }

        public async Task<CustomerSearchResponse> SearchAsyncv2(CustomerSearchRequest request)
        {
            var extendJoins = new List<string>();
            var conditions = new List<string> { };
            //var extendJoinKeys = request.SqlQueryParam.GetJoinKeys();
            //search on column
            var filterContdition = new List<string>();
            var parameters = new DynamicParameters();
            if (request.Condition.SearchText == null)
            {
                request.Condition.SearchText = "";
            }
            string SearchText = request.Condition.SearchText.ToLower();
            string TextPattern = request.Condition.SearchText.ToUpper();
            string TextPatternFreetext = request.Condition.SearchText.ToUpper().ToFullSearch();
            string MemberNumber = "";
            string SiteId = "";
            string Processor = request.Condition.Processor ?? "";
            string Type = request.Condition.Type ?? "";
            int DateRange = request.Condition.RemainingDays ?? 0;
            int Count = request.PageSize;
            int Page = request.PageIndex;
            var merchantStatus = request.Condition.MerchantStatus;
            if (merchantStatus == null)
            {
                merchantStatus = new List<int> { 0 };
            }

            int IsSoft = merchantStatus.Where(x => x == 2).Count();
            int IsTerminal = merchantStatus.Where(x => x == 3).Count();
            int IsTerminalSoft = merchantStatus.Where(x => x == 4).Count();
            int IsCancel = merchantStatus.Where(x => x == 8).Count();

            if (Type == null || Type.Trim() == "")
            {
                Type = "STORE_OF_MERCHANT";
            }
            if (request.Condition.AccountManagers != null && request.Condition.AccountManagers.Count >= 1)
            {
                MemberNumber = String.Join(",", request.Condition.AccountManagers);
            }
            if (request.Condition.SiteIds != null && request.Condition.SiteIds.Count >= 1)
            {
                SiteId = String.Join(",", request.Condition.SiteIds);
            }
            string sqlQuery = $"exec P_MerchantDashboard '{TextPatternFreetext}', '{SearchText}', '{TextPattern}', '{MemberNumber}', '{SiteId}', '{Processor}','{Type}','{DateRange}','{IsCancel}','{IsSoft}','{IsTerminal}','{IsTerminalSoft}','{Page}','{Count}'";
            List<CustomerListItemDto> lstResult = (await _connection.QueryAsync<CustomerListItemDto>(sqlQuery)).ToList();

            var response = new CustomerSearchResponse();
            //response.Records = request.PageSize;
            //response.Pagination = request.PageIndex;
            int totalRecords = lstResult.Count;
            if (totalRecords != 0)
            {
                totalRecords = lstResult.FirstOrDefault().MaxRows;
            }
            response.Pagination = new PaginationDto { TotalRecords = totalRecords };
            response.Records = lstResult;


            return response;
        }

        public async Task<StoreLicenses> getCustomerInfoByCode(string CustomerCode, string OrderCode)
        {
            return await _connection.SqlFirstOrDefaultAsync<StoreLicenses>("exec P_GetCustomerInfoByCode @CustomerCode, @OrderCode", new { CustomerCode, OrderCode });
        }
    }



}
