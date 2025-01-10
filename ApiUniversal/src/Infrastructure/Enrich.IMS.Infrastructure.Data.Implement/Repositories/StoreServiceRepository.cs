using Dapper;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto;
using Enrich.Dto.Base.POSApi;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Subscription;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using System.Transactions;
using Twilio.Http;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public class StoreServiceRepository : DapperGenericRepository<StoreService>, IStoreServiceRepository
    {
        private readonly EnrichContext _context;
        public StoreServiceRepository(IConnectionFactory connectionFactory, EnrichContext context)
            : base(connectionFactory)
        {
            _context = context;
        }
        public IEnumerable<StoreService> GetByOrderCode(string orderCode)
        {
            var query = $"SELECT TOP 1 * FROM {SqlTables.StoreServices} WITH (NOLOCK) WHERE [OrderCode] = @orderCode";
            var parameters = new DynamicParameters();
            parameters.Add("orderCode", orderCode);
            using (var connection = GetDbConnection())
            {
                return connection.Query<StoreService>(query, parameters);
            }
        }


        public async Task WriteLog(string JsonRequest, string JsonRespone, string Url, string RequestUrl, string RequestMethod, string StatusCode, string SalonName)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var connection = GetDbConnection())
                {
                    string sqlQuery = $"exec WriteLogIMS '{RequestMethod}','{Url}','{RequestUrl}','{StatusCode}','{JsonRequest}','{JsonRespone}','{SalonName}' ";
                    await connection.ExecuteAsync(sqlQuery);
                }
                scope.Complete();
            }
        }
        public StoreService GetByOrderCodeNSubscriptionCode(string orderCode, string subscriptionCode)
        {
            var query = $"SELECT TOP 1 * FROM {SqlTables.StoreServices} WITH (NOLOCK) WHERE [OrderCode] = @orderCode AND [ProductCode] = @subscriptionCode";
            var parameters = new DynamicParameters();
            parameters.Add("orderCode", orderCode);
            parameters.Add("subscriptionCode", subscriptionCode);
            using (var connection = GetDbConnection())
            {
                return connection.QueryFirstOrDefault<StoreService>(query, parameters);
            }
        }
        public async Task<StoreService> GetByLastOrderCodeAsync(string lastOrderCode, string subscriptionCode)
        {
            var query = $"SELECT TOP 1 * FROM {SqlTables.StoreServices} WITH (NOLOCK) WHERE Active = @activeStatus AND [LastRenewOrderCode] = @lastOrderCode and ProductCode = @subscriptionCode";
            var parameters = new DynamicParameters();
            parameters.Add("activeStatus", (int)SubscriptionEnum.Status.Active);
            parameters.Add("lastOrderCode", lastOrderCode);
            parameters.Add("subscriptionCode", subscriptionCode);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<StoreService>(query, parameters);
            }
        }
        public async Task<StoreService> GetLicenseActivatedAsync(string customerCode, string subscriptionCode)
        {
            var query = $"SELECT TOP 1 * FROM {SqlTables.StoreServices} WITH (NOLOCK) WHERE Type = @licenseType AND Active = @activeStatus AND CustomerCode = @customerCode and ProductCode = @subscriptionCode";
            var parameters = new DynamicParameters();
            parameters.Add("licenseType", SubscriptionEnum.Type.license.ToString());
            parameters.Add("activeStatus", (int)SubscriptionEnum.Status.Active);
            parameters.Add("customerCode", customerCode);
            parameters.Add("subscriptionCode", subscriptionCode);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<StoreService>(query, parameters);
            }
        }
        public async Task<StoreService> GetLicenseActivatedAsync(string customerCode)
        {
            var query = $"SELECT TOP 1 * FROM {SqlTables.StoreServices} WITH (NOLOCK) WHERE Type = @licenseType AND Active = @activeStatus AND CustomerCode = @customerCode";
            var parameters = new DynamicParameters();
            parameters.Add("licenseType", SubscriptionEnum.Type.license.ToString());
            parameters.Add("activeStatus", (int)SubscriptionEnum.Status.Active);
            parameters.Add("customerCode", customerCode);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<StoreService>(query, parameters);
            }
        }
        public async Task<IEnumerable<BaseService>> GetBaseServiceAsync(string customerCode, string storeServiceId, bool addon = false)
        {
            string query = string.Empty;
            if (!addon)
            {
                query = @"SELECT lpi.License_Item_Code as LicenseCode,
		                        MAX(li.Type) as LicenseType,
		                        SUM(lpi.Value) as Count_limit,
		                        FORMAT(MAX(ss.EffectiveDate), 'MM/dd/yyyy') as Start_period,
		                        FORMAT(MAX(ss.RenewDate), 'MM/dd/yyyy') as End_period
                        FROM (SELECT * FROM " + SqlTables.StoreServices + @" WHERE CustomerCode = @customerCode AND Id = @storeServiceId) as ss
                        JOIN " + SqlTables.LicenseProduct + @" as lp ON ss.ProductCode = lp.Code
                        JOIN " + SqlTables.LicenseProductItem + @" as lpi ON lp.Id = lpi.License_Product_Id
                        JOIN " + SqlTables.LicenseItem + @" as li ON lpi.License_Item_Code = li.Code
                        WHERE ss.EffectiveDate <= GETUTCDATE() AND 
                                li.GroupName = @groupName AND 
                                li.Enable = 1 and lpi.Enable = 1
                        GROUP BY lpi.License_Item_Code";
            }
            else
            {
                query = @"SELECT lpi.License_Item_Code as LicenseCode,
		                        MAX(li.Type) as LicenseType, 
		                        SUM(lpi.Value) as Count_limit, 
		                        FORMAT(MAX(ss.EffectiveDate), 'MM/dd/yyyy') as Start_period,
		                        FORMAT(MAX(ss.RenewDate), 'MM/dd/yyyy') as End_period
                        FROM (SELECT * FROM " + SqlTables.StoreServices + @" WHERE CustomerCode = @customerCode AND (Id = @storeServiceId OR (Active = @subscriptionActive AND Type = @addon))) as ss
                        JOIN " + SqlTables.LicenseProduct + @" as lp ON ss.ProductCode = lp.Code
                        JOIN " + SqlTables.LicenseProductItem + @" as lpi ON lp.Id = lpi.License_Product_Id
                        JOIN " + SqlTables.LicenseItem + @" as li ON lpi.License_Item_Code = li.Code
                        WHERE ss.EffectiveDate <= GETUTCDATE() AND 
                                li.GroupName = @groupName AND 
                                li.Enable = 1 and lpi.Enable = 1
                        GROUP BY lpi.License_Item_Code";
            }
            var parameters = new DynamicParameters();
            parameters.Add("customerCode", customerCode);
            parameters.Add("storeServiceId", storeServiceId);
            parameters.Add("subscriptionActive", (int)SubscriptionEnum.Status.Active);
            parameters.Add("addon", SubscriptionEnum.Type.addon.ToString());
            parameters.Add("groupName", EnumHelper.DisplayName(SubscriptionEnum.GroupName.BaseService));
            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<BaseService>(query, parameters);
            }
        }
        //get list base service on activated
        public async Task<IEnumerable<BaseService>> GetBaseServiceAsync(string customerCode)
        {
            var query = @"WITH get_license_active AS (SELECT * FROM " + SqlTables.StoreServices + @" WHERE CustomerCode = @customerCode AND Active = @subscriptionActive and EffectiveDate <= GETUTCDATE())
                        SELECT lpi.License_Item_Code as LicenseCode,
			                MIN (li.Type) as LicenseType,
			                CASE 
				                WHEN MIN(lpi.Value) = -1 THEN -1
				                ELSE SUM (lpi.Value)
			                END as Count_limit,
		                        FORMAT(MAX(ss.EffectiveDate), 'MM/dd/yyyy') as Start_period,
		                        FORMAT(MAX(ss.RenewDate), 'MM/dd/yyyy') as End_period
	                    FROM get_license_active as ss
		                    JOIN " + SqlTables.LicenseProduct + @" as lp ON ss.ProductCode = lp.Code
		                    JOIN " + SqlTables.LicenseProductItem + @" as lpi ON lp.Id = lpi.License_Product_Id
		                    JOIN " + SqlTables.LicenseItem + @" as li ON lpi.License_Item_Code = li.Code
	                    WHERE li.GroupName = @groupName AND 
			                    li.Enable = 1 and lpi.Enable = 1
	                    GROUP BY lpi.License_Item_Code";
            var parameters = new DynamicParameters();
            parameters.Add("customerCode", customerCode);
            parameters.Add("groupName", EnumHelper.DisplayName(SubscriptionEnum.GroupName.BaseService));
            parameters.Add("subscriptionActive", (int)SubscriptionEnum.Status.Active);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<BaseService>(query, parameters);
                //return data.GroupBy(st => st.LicenseCode).Select(rs => new BaseService
                //{
                //    LicenseCode = rs.Key,
                //    LicenseType = rs.FirstOrDefault().LicenseType,
                //    Count_limit = rs.Where(x => x.Count_limit == -1).Count() > 0 ? -1 : rs.Where(item => item.Count_limit > 0).Sum(item => item.Count_limit),
                //    Start_period = rs.Max(item => item.Start_period),
                //    End_period = rs.Max(item => item.End_period)
                //});
            }
        }

        public async Task<IEnumerable<BaseService>> GetBaseServiceByStoreServiceIdAsync(string storeServiceId)
        {
            var query = @"WITH get_license_active AS (SELECT * FROM " + SqlTables.StoreServices + @" WHERE [Id] = @storeServiceId)
                        SELECT lpi.License_Item_Code as LicenseCode,
			                MIN (li.Type) as LicenseType,
			                CASE 
				                WHEN MIN(lpi.Value) = -1 THEN -1
				                ELSE SUM (lpi.Value)
			                END as Count_limit,
		                        FORMAT(MAX(ss.EffectiveDate), 'MM/dd/yyyy') as Start_period,
		                        FORMAT(MAX(ss.RenewDate), 'MM/dd/yyyy') as End_period
	                    FROM get_license_active as ss
		                    JOIN " + SqlTables.LicenseProduct + @" as lp ON ss.ProductCode = lp.Code
		                    JOIN " + SqlTables.LicenseProductItem + @" as lpi ON lp.Id = lpi.License_Product_Id
		                    JOIN " + SqlTables.LicenseItem + @" as li ON lpi.License_Item_Code = li.Code
	                    WHERE li.GroupName = @groupName AND 
			                    li.Enable = 1 and lpi.Enable = 1
	                    GROUP BY lpi.License_Item_Code";
            var parameters = new DynamicParameters();
            parameters.Add("storeServiceId", storeServiceId);
            parameters.Add("groupName", EnumHelper.DisplayName(SubscriptionEnum.GroupName.BaseService));
            parameters.Add("subscriptionActive", (int)SubscriptionEnum.Status.Active);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<BaseService>(query, parameters);
            }
        }
        public async Task<IEnumerable<FeatureBase>> GetFeatureAsync(string customerCode)
        {
            var query = @"SELECT lpi.License_Item_Code, lpi.Value, ss.Quantity, ss.Type as SupscriptionType, li.Type as FeatureType
                        FROM (SELECT * FROM " + SqlTables.StoreServices + @" WHERE CustomerCode = @customerCode AND Active = @subscriptionActive) as ss
                        JOIN " + SqlTables.LicenseProduct + @" as lp ON ss.ProductCode = lp.Code
                        JOIN " + SqlTables.LicenseProductItem + @" as lpi ON lp.Id = lpi.License_Product_Id
                        JOIN " + SqlTables.LicenseItem + @" as li ON lpi.License_Item_Code = li.Code
                        WHERE ss.EffectiveDate <= GETUTCDATE() AND li.GroupName = @groupName AND lpi.Enable = 1";
            var parameters = new DynamicParameters();
            parameters.Add("customerCode", customerCode);
            parameters.Add("groupName", EnumHelper.DisplayName(SubscriptionEnum.GroupName.Feature));
            parameters.Add("subscriptionActive", (int)SubscriptionEnum.Status.Active);
            using (var connection = GetDbConnection())
            {
                var result = await connection.QueryAsync<FeatureBase>(query, parameters);
                return result;
            }
        }

        public async Task<StoreService> UpdateGetRecurringAsync(string orderCode, string orderCodeRecurring, string subscriptionCode)
        {
            var storeService = GetByOrderCodeNSubscriptionCode(orderCode, subscriptionCode);
            storeService.LastRenewAt = DateTime.UtcNow;
            storeService.LastRenewBy = _context.UserFullName;
            storeService.HasRenewInvoiceIncomplete = true;
            storeService.LastRenewOrderCode = orderCodeRecurring;
            await UpdateAsync(storeService);
            return storeService;
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
            using (var connection = GetDbConnection())
            {
                var result = await connection.QueryAsync<LicenseStatusDto>(query, new
                {
                    storeCodes = listStoreCode,
                    storeServiceStatus = storeServiceStatus
                });
                return result;
            }
        }
    }
}
