using Dapper;
using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto.Base.Parameters;
using Enrich.Dto.List;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class CustomerRepository
    {
        public async Task<CustomerSearchResponse> SearchAsync(CustomerSearchRequest request)
        {
            var extendJoins = new List<string>();
            var conditions = new List<string> { };
            var extendJoinKeys = request.SqlQueryParam.GetJoinKeys();
            //search on column
            var filterContdition = new List<string>();
            var parameters = new DynamicParameters();
            if (request.Condition == null)
            {
                goto ExecuteSearch;
            }

            // filter on search zone
            PopulateConditionCommon(request.Condition, extendJoins, extendJoinKeys, conditions, parameters);

            // filter by serach text
            PopulateConditionFullTextSearch(request.Condition, extendJoins, extendJoinKeys, conditions, parameters);

            // filter by column
            PopulateFilterConditionCommon(request.FilterCondition, filterContdition, parameters);

            // filter by service type
            //PopulateConditionMerchantServiceType(request.Condition, extendJoins, extendJoinKeys, conditions, parameters);

            // filter by merchant status. live or not live
            PopulateConditionMerchantStatus(request.Condition, extendJoins, extendJoinKeys, conditions, parameters);

            //filter by license status
            //PopulateConditionLicenseStatus(request.Condition, extendJoins, extendJoinKeys, conditions, parameters);

            //filter by terminal status
            //PopulateConditionTerminalStatus(request.Condition, extendJoins, extendJoinKeys, conditions, parameters);

            //filter by remainning date
            PopulateConditionRemainingDate(request.Condition, extendJoins, extendJoinKeys, conditions, parameters);

            PopulateConditionAtRisk(request.Condition, extendJoins, extendJoinKeys, conditions, parameters);

            PopulateConditionCreateAt(request.Condition, extendJoins, extendJoinKeys, conditions, parameters);

            //get summary query
            PopulateSummaryByMerchantType(request, extendJoins, extendJoinKeys, conditions, parameters);

            PopulateMerchantType(request.Condition, extendJoins, extendJoinKeys, conditions, parameters);

        ExecuteSearch:

            SqlScript.PopulateTemplateFields(request.SqlQueryParam.Fields);
            //if (extendJoinKeys.Count > 0) extendJoins.AddRange(SqlScript.GetJoins(SqlScript.Ticket.JoinsSearch, extendJoinKeys));

            var parserQuery = new SqlParserParameter()
            {
                ExtendJoins = extendJoins,
                Conditions = conditions,
                FilterResultByColumnConditions = filterContdition,
                QueryParam = request.SqlQueryParam,
                QueryTemplate = SqlScript.QuerySearch(Alias)
            };

            if (extendJoinKeys.Count > 0)
            {
                extendJoinKeys = extendJoinKeys.Distinct().ToList();
                extendJoins.AddRange(SqlScript.GetJoins(SqlScript.Customer.JoinsSearch, extendJoinKeys));
            }

            var sqlInfo = ParseSqlQueryInfoByTemplateCTE(parserQuery);

            //final
            var response = new CustomerSearchResponse();
            if (request.SqlQueryParam.IsOnlyGetTotalRecords)
            {
                var queryTotalRecords = sqlInfo.PopulateQueryExtend(sqlInfo.QueryTotalRecords);
                var totalRecords = await GetScalarValueAsync<int>(queryTotalRecords, parameters);
                response.Pagination = new PaginationDto { TotalRecords = totalRecords };
            }
            else
            {
                var exeResponse = await ExecuteQueryCTEAsync<CustomerListItemDto>(sqlInfo, request.SqlQueryParam.Paging, parameters, reader =>
                {
                    if (request.Condition != null && request.Condition.PopulateCountSummaries)
                    {
                        PopupateCountSummariesAsync(response, reader);
                    }
                });
                response.Records = exeResponse.Records;
                response.Pagination = exeResponse.Pagination;

                var tasks = new MultipleTasks();
                tasks.Add(GetScalarValueAsync<int>(request.SummaryQuery.QueryTotalStoreOfMerchant, parameters),
                    val => response.Summary.TotalStoreOfMerchant = (int)val);
                tasks.Add(GetScalarValueAsync<int>(request.SummaryQuery.QueryTotalStoreInHouse, parameters),
                    val => response.Summary.TotalStoreInHouse = (int)val);
                await tasks.WhenAll();
            }
            return response;
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
            string Processor = request.Condition.Processor??"";
            string Type = request.Condition.Type ?? "";
            int DateRange = request.Condition.RemainingDays??1;
            int Count = request.PageSize;
            int Page = request.PageIndex;
            var merchantStatus = request.Condition.MerchantStatus;
            if(merchantStatus == null)
            {
                merchantStatus =new List<int> { 0 };
            }

            int IsSoft = merchantStatus.Where(x=>x == 2).Count();
            int IsTerminal = merchantStatus.Where(x => x == 3).Count();
            int IsTerminalSoft = merchantStatus.Where(x => x == 4).Count();
            int IsCancel = merchantStatus.Where(x => x == 8).Count();

            if (DateRange == (int)CustomerEnum.RemainingDaySearch.Day_7)
            {
                DateRange = 7;
            }
            else if(DateRange == (int)CustomerEnum.RemainingDaySearch.Day_14)
            {
                DateRange = 14;
            }
            else if (DateRange == (int)CustomerEnum.RemainingDaySearch.Month_1)
            {
                DateRange = 30;
            }
            else if (DateRange == (int)CustomerEnum.RemainingDaySearch.Month_2)
            {
                DateRange = 60;
            }
            else if (DateRange == (int)CustomerEnum.RemainingDaySearch.Month_3)
            {
                DateRange = 90;
            }
            else if (DateRange == (int)CustomerEnum.RemainingDaySearch.Month_4)
            {
                DateRange = 120;
            }
            else if (DateRange == (int)CustomerEnum.RemainingDaySearch.Month_6)
            {
                DateRange = 180;
            }
            else if (DateRange == (int)CustomerEnum.RemainingDaySearch.Month_12)
            {
                DateRange = 365;
            }
            if (Type==null || Type.Trim() == "")
            {
                Type = "STORE_OF_MERCHANT";
            }
            if (request.Condition.AccountManagers!= null && request.Condition.AccountManagers.Count >= 1)
            {
                MemberNumber = String.Join(",", request.Condition.AccountManagers);
            }
            if (request.Condition.SiteIds != null && request.Condition.SiteIds.Count >= 1)
            {
                SiteId = String.Join(",", request.Condition.SiteIds);
            }
            string sqlQuery = $"exec P_MerchantDashboard '{TextPatternFreetext}', '{SearchText}', '{TextPattern}', '{MemberNumber}', '{SiteId}', '{Processor}','{Type}','{DateRange}','{IsCancel}','{IsSoft}','{IsTerminal}','{IsTerminalSoft}','{Page}','{Count}'";
            

            List<CustomerListItemDto> lstResult = await ExecuteQuerySearchMechant(sqlQuery);
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
        /// <summary>
        /// Get merchant sumary. Need to improve
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        //public async Task<CustomerSearchSummary> GetMerchantSumaryAsync(CustomerSearchRequest request)
        //{
        //    var result = new CustomerSearchSummary();
        //    request.IsOnlyGetTotalRecords = true;
        //    if (request.Condition == null)
        //        request.Condition = new CustomerSearchCondition();

        //    var tasks = new MultipleTasks();

        //    // get terminalMerchant record
        //    var tsMerchantRequest = request.Clone();
        //    tsMerchantRequest.Condition.ServiceType = (int)CustomerEnum.ServiceType.Merchant_License_MID;
        //    var softwareTerminalMerchantSearchResult =  SearchAsync(tsMerchantRequest);
        //    tasks.Add(softwareTerminalMerchantSearchResult, val => result.TotalSoftwareTerminalMerchant = (val as CustomerSearchResponse)?.Pagination.TotalRecords ?? 0);

        //    // get terminalMerchant record
        //    var sMerchantRequest = request.Clone();
        //    sMerchantRequest.Condition.ServiceType = (int)CustomerEnum.ServiceType.Merchant_License;
        //    var softwareMerchantSearchResult =  SearchAsync(sMerchantRequest);
        //    tasks.Add(softwareMerchantSearchResult, val => result.TotalSoftwareMerchant = (val as CustomerSearchResponse)?.Pagination.TotalRecords ?? 0);

        //    // get terminalMerchant record
        //    var tMerchantRequest = request.Clone();
        //    tMerchantRequest.Condition.ServiceType = (int)CustomerEnum.ServiceType.Merchant_MID;
        //    var terminalMerchantSearchResult =  SearchAsync(tMerchantRequest);
        //    tasks.Add(terminalMerchantSearchResult, val => result.TotalTerminalMerchant = (val as CustomerSearchResponse)?.Pagination.TotalRecords ?? 0);

        //    // get all merchant record
        //    var allMerchantRequest = request.Clone();
        //    allMerchantRequest.Condition.ServiceType = null;
        //    var allMerchantSearchResult =  SearchAsync(allMerchantRequest);
        //    tasks.Add(allMerchantSearchResult, val => result.TotalMerchant = (val as CustomerSearchResponse)?.Pagination.TotalRecords??0);

        //    await tasks.WhenAll();

        //    return result;
        //}

        private void PopulateConditionCommon(CustomerSearchCondition request, List<string> extendJoins, List<string> extendJoinKeys, List<string> conditions, DynamicParameters parameters)
        {
            extendJoinKeys.Add(SqlScript.Customer.JoinKeys.CustomerWTransaction);
            AddConditionInList(conditions, $"{Alias}.Id", request.Ids);
            AddConditionNotInList(conditions, $"{Alias}.Id", request.ExcludeIds);

            if (request.AccountManagers != null)
                AddConditionInList(conditions, $"{Alias}.MemberNumber", request.AccountManagers);

            if (request.SiteIds != null)
                AddConditionInList(conditions, $"{Alias}.SiteId", request.SiteIds.ToArray());

            if (!string.IsNullOrEmpty(request.State))
                conditions.Add($"({SqlTables.Customer}.[State] = '{request.State}' OR {SqlTables.Customer}.[BusinessState] = '{request.State}' OR {SqlTables.Customer}.[SalonState] = '{request.State}')");

            if (!string.IsNullOrEmpty(request.City))
                conditions.Add($"({SqlTables.Customer}.[City] = '{request.City}' OR {SqlTables.Customer}.[BusinessCity] = '{request.City}' OR {SqlTables.Customer}.[SalonCity] = '{request.City}')");

            if (!string.IsNullOrEmpty(request.ZipCode))
                conditions.Add($"({SqlTables.Customer}.[ZipCode] = '{request.ZipCode}' OR {SqlTables.Customer}.[BusinessZipCode] = '{request.ZipCode}' OR {SqlTables.Customer}.[SalonZipCode] = '{request.ZipCode}')");

            if (!string.IsNullOrEmpty(request.Processor))
                conditions.Add($"({SqlTables.Customer}.[Processor] = '{request.Processor}')");
        }

        private void PopulateConditionFullTextSearch(CustomerSearchCondition request, List<string> extendJoins, List<string> extendJoinKeys, List<string> conditions, DynamicParameters parameters)
        {
            if (!String.IsNullOrEmpty(request.SearchText))
            {
                request.SearchText = request.SearchText.Trim();
                extendJoinKeys.Add("FullTextSearch");
                parameters.Add("TextPattern", $"IsAbout(\"*{request.SearchText.ToUpper()}*\") | \"{request.SearchText.ToUpper()}\"", System.Data.DbType.AnsiString);
                parameters.Add("TextPatternFreetext", $"{request.SearchText.ToUpper().ToFullSearch()}", System.Data.DbType.AnsiString);
                parameters.Add("SearchText", request.SearchText.ToLower());
                
            }
        }

        //refer: merchant type. License, Mid, software and mid
        private void PopulateConditionMerchantServiceType(CustomerSearchCondition request, List<string> extendJoins, List<string> extendJoinKeys, List<string> conditions, DynamicParameters parameters)
        {
            int[] storeServiceStatus = { (int)SubscriptionEnum.Status.Active, (int)SubscriptionEnum.Status.Deactive };

            if (request.ServiceType != null)
            {
                if (request.ServiceType == (int)CustomerEnum.ServiceType.Merchant_License_MID)
                {
                    conditions.Add($" {SqlTables.Customer}.MID IS NOT NUll ");

                    AddConditionInList(conditions, $"{SqlTables.StoreServices}.Active", storeServiceStatus);
                }
                else if (request.ServiceType == (int)CustomerEnum.ServiceType.Merchant_License)
                {
                    conditions.Add($" ( {SqlTables.Customer}.MID IS NUll OR TerminalStatus = 0 OR TerminalStatus is NULL ) ");
                    conditions.Add($" {SqlTables.StoreServices}.Active IN ({storeServiceStatus.ToStringList()}) ");
                }
                else if (request.ServiceType == (int)CustomerEnum.ServiceType.Merchant_MID)
                {
                    conditions.Add($" {SqlTables.Customer}.MID IS NOT NUll ");
                    conditions.Add($" {SqlTables.Customer}.CustomerCode NOT IN (SELECT CustomerCode FROM {SqlTables.StoreServices} WHERE Active = 1) ");
                }
            }
            else
            {
                conditions.Add($"( {SqlTables.Customer}.MID IS NOT NUll OR  {SqlTables.StoreServices}.Active IN ({storeServiceStatus.ToStringList()}) ) ");
            }
        }

        //refer: active, inaactivate
        private void PopulateConditionLicenseStatus(CustomerSearchCondition request, List<string> extendJoins, List<string> extendJoinKeys, List<string> conditions, DynamicParameters parameters)
        {
            if (request.LicenseStatus == null)
                return;

            if (request.LicenseStatus == (int)CustomerEnum.StatusSearch.Expires)
            {
                conditions.Add($"{SqlTables.StoreServices}.Active = 1 AND  {SqlTables.StoreServices}.RenewDate < CONVERT(date ,GETUTCDATE())");
            }
            else if (request.LicenseStatus == (int)CustomerEnum.StatusSearch.Active)
            {
                conditions.Add($"{SqlTables.StoreServices}.Active = 1 AND {SqlTables.StoreServices}.RenewDate >= CONVERT(date ,GETUTCDATE())");
                conditions.Add($"( {SqlTables.Customer}.CancelDate IS NULL OR  {SqlTables.Customer}.CancelDate > GETUTCDATE() )");
            }
            else if (request.LicenseStatus == (int)CustomerEnum.StatusSearch.Deactive)
            {
                conditions.Add($" {SqlTables.Customer}.CustomerCode NOT IN (SELECT CustomerCode FROM Store_Services WHERE Active = 1) ");
            }
            else if (request.LicenseStatus == (int)CustomerEnum.StatusSearch.Cancle)
            {
                conditions.Add($" (C_Customer.CancelDate is not null AND C_Customer.CancelDate<= GETUTCDATE()) ");
            }
        }

        //refer: active, inaactivate
        private void PopulateConditionMerchantStatus(CustomerSearchCondition request, List<string> extendJoins, List<string> extendJoinKeys, List<string> conditions, DynamicParameters parameters)
        {
            conditions.Add(SqlScript.Customer.Conditions.IsExistStoreCode);
            conditions.Add(SqlScript.Customer.Conditions.HaveTransaction);

            if (request.MerchantStatus == null)
                return;

            var customizeConditions = new List<string>();

            if (request.MerchantStatus.IndexOf((int)CustomerEnum.MerchantStatusSearch.LiveMerchant) >= 0)
            {
                //conditions.Add(SqlScript.Customer.Conditions.Merchant);
                //conditions.Add(SqlScript.Customer.Conditions.MerchantIsNotCanceled);
                customizeConditions.Add($@"({SqlScript.Customer.Conditions.Merchant} 
                            AND {SqlScript.Customer.Conditions.MerchantIsNotCanceled})");
            }

            if (request.MerchantStatus.IndexOf((int)CustomerEnum.MerchantStatusSearch.OnlyLicense) >= 0)
            {
                //conditions.Add(SqlScript.Customer.Conditions.MerchantLicense);
                //conditions.Add(SqlScript.Customer.Conditions.MerchantIsNotCanceled);

                customizeConditions.Add($@"({SqlScript.Customer.Conditions.MerchantLicense} 
                            AND {SqlScript.Customer.Conditions.MerchantIsNotCanceled})");
            }
            
            if (request.MerchantStatus.IndexOf((int)CustomerEnum.MerchantStatusSearch.OnlyTerminal) >= 0)
            {
                //conditions.Add(SqlScript.Customer.Conditions.MerchantMID);
                //conditions.Add(SqlScript.Customer.Conditions.MerchantIsNotCanceled);

                customizeConditions.Add($@"({SqlScript.Customer.Conditions.MerchantMID} 
                            AND {SqlScript.Customer.Conditions.MerchantIsNotCanceled})");
            }
            
            if (request.MerchantStatus.IndexOf((int)CustomerEnum.MerchantStatusSearch.LicenseTerminal) >= 0)
            {
                //conditions.Add(SqlScript.Customer.Conditions.MerchantLicenseAndMID);

                customizeConditions.Add($@"({SqlScript.Customer.Conditions.MerchantLicenseAndMID} 
                            AND {SqlScript.Customer.Conditions.MerchantIsNotCanceled})");
            }
            
            if (request.MerchantStatus.IndexOf((int)CustomerEnum.MerchantStatusSearch.ExpiredLicenseActiveTerminal) >= 0)
            {
                //conditions.Add(SqlScript.Customer.Conditions.HaveLicenseExpires);
                //conditions.Add(SqlScript.Customer.Conditions.HaveMID);
                //conditions.Add(SqlScript.Customer.Conditions.MerchantIsNotCanceled);

                customizeConditions.Add($@"({SqlScript.Customer.Conditions.HaveLicenseExpires} 
                            AND {SqlScript.Customer.Conditions.HaveMID}
                            AND {SqlScript.Customer.Conditions.MerchantIsNotCanceled})");
            }
            
            if (request.MerchantStatus.IndexOf((int)CustomerEnum.MerchantStatusSearch.ExpiredLicenseInactiveTerminal) >= 0)
            {
                //conditions.Add(SqlScript.Customer.Conditions.HaveLicenseExpires);
                //conditions.Add(SqlScript.Customer.Conditions.DontHaveMID);
                //conditions.Add(SqlScript.Customer.Conditions.MerchantIsNotCanceled);

                customizeConditions.Add($@"({SqlScript.Customer.Conditions.HaveLicenseExpires} 
                            AND {SqlScript.Customer.Conditions.DontHaveMID}
                            AND {SqlScript.Customer.Conditions.MerchantIsNotCanceled})");
            }
            
            if (request.MerchantStatus.IndexOf((int)CustomerEnum.MerchantStatusSearch.InactiveLicenseInactiveTerminal) >= 0)
            {
                //conditions.Add(SqlScript.Customer.Conditions.DontHaveLicenseActive);
                //conditions.Add(SqlScript.Customer.Conditions.DontHaveMID);
                //conditions.Add(SqlScript.Customer.Conditions.MerchantIsNotCanceled);

                customizeConditions.Add($@"({SqlScript.Customer.Conditions.DontHaveLicenseActive} 
                            AND {SqlScript.Customer.Conditions.DontHaveMID}
                            AND {SqlScript.Customer.Conditions.MerchantIsNotCanceled})");
            }
            
            if (request.MerchantStatus.IndexOf((int)CustomerEnum.MerchantStatusSearch.Cancel) >= 0)
            {
                //conditions.Add(SqlScript.Customer.Conditions.MerchantIsCanceled);
                customizeConditions.Add($@"({SqlScript.Customer.Conditions.MerchantIsCanceled})");
            }

            conditions.Add($"({string.Join(" OR ", customizeConditions)})");
        }

        private void PopulateConditionTerminalStatus(CustomerSearchCondition request, List<string> extendJoins, List<string> extendJoinKeys, List<string> conditions, DynamicParameters parameters)
        {
            if (request.TerminalStatus == null)
                return;

            if (request.TerminalStatus == (int)SubscriptionEnum.TerminalStatus.Active)
            {
                conditions.Add($"( {SqlTables.Customer}.MID IS NOT NULL AND ( {SqlTables.Customer}.TerminalStatus IS NULL OR {SqlTables.Customer}.TerminalStatus = 1 )) ");
            }
            else
            {
                conditions.Add($"( {SqlTables.Customer}.MID IS  NULL OR  {SqlTables.Customer}.TerminalStatus = 0 )");
            }
        }

        private void PopulateConditionAtRisk(CustomerSearchCondition request, List<string> extendJoins, List<string> extendJoinKeys, List<string> conditions, DynamicParameters parameters)
        {
            if (request.AtRisk != null)
            {
                if (request.AtRisk == (int)CustomerEnum.AtRiskSearch.Expried_In_3Day)
                {
                    var expiresDay = 3;

                    conditions.Add($"{SqlTables.StoreServices}.Active = 1 AND  DATEDIFF(day, {SqlTables.StoreServices}.RenewDate,CONVERT(date ,GETUTCDATE())) <= {expiresDay} ");
                }
                else if (request.AtRisk == (int)CustomerEnum.AtRiskSearch.Dont_Have_Card)
                {
                    conditions.Add($"{SqlTables.Customer}.CustomerCode = NOT IN (SELECT CustomerCode FROM {SqlTables.CustomerCard} WHERE {SqlTables.CustomerCard}.IsDefault = 1 ) ");
                }
            }
        }

        private void PopulateConditionCreateAt(CustomerSearchCondition request, List<string> extendJoins, List<string> extendJoinKeys, List<string> conditions, DynamicParameters parameters)
        {
            if (request.NODaysCreated != null)
            {
                var targetTypeDate = "day";
                var differentDay = 0;
                if (request.RemainingDays == (int)CustomerEnum.RemainingDaySearch.Day_7)
                {
                    targetTypeDate = "day";
                    differentDay = 7;
                }
                else if (request.RemainingDays == (int)CustomerEnum.RemainingDaySearch.Day_14)
                {
                    targetTypeDate = "day";
                    differentDay = 14;
                }
                else if (request.RemainingDays == (int)CustomerEnum.RemainingDaySearch.Month_1)
                {
                    targetTypeDate = "month";
                    differentDay = 1;
                }
                else if (request.RemainingDays == (int)CustomerEnum.RemainingDaySearch.Month_3)
                {
                    targetTypeDate = "month";
                    differentDay = 3;
                }
                else if (request.RemainingDays == (int)CustomerEnum.RemainingDaySearch.Month_4)
                {
                    targetTypeDate = "month";
                    differentDay = 5;
                }
                else if (request.RemainingDays == (int)CustomerEnum.RemainingDaySearch.All)
                {
                    targetTypeDate = "month";
                    differentDay = 6;
                }
                else if (request.RemainingDays == (int)CustomerEnum.RemainingDaySearch.Month_12)
                {
                    targetTypeDate = "month";
                    differentDay = 12;
                }
                conditions.Add($" DATEDIFF({targetTypeDate},{SqlTables.Customer}.CreateAt,CONVERT(date ,GETUTCDATE())) >= {differentDay} ");
            }
        }

        private void PopulateConditionRemainingDate(CustomerSearchCondition request, List<string> extendJoins, List<string> extendJoinKeys, List<string> conditions, DynamicParameters parameters)
        {
            if (request.RemainingDays != null)
            {

                var targetTypeDate = "day";
                var differentDay = 0;
                if (request.RemainingDays == (int)CustomerEnum.RemainingDaySearch.Day_7)
                {
                    targetTypeDate = "day";
                    differentDay = 7;
                }
                else if (request.RemainingDays == (int)CustomerEnum.RemainingDaySearch.Day_14)
                {
                    targetTypeDate = "day";
                    differentDay = 14;
                }
                else if (request.RemainingDays == (int)CustomerEnum.RemainingDaySearch.Month_1)
                {
                    targetTypeDate = "month";
                    differentDay = 1;
                }
                else if (request.RemainingDays == (int)CustomerEnum.RemainingDaySearch.Month_3)
                {
                    targetTypeDate = "month";
                    differentDay = 3;
                }
                else if (request.RemainingDays == (int)CustomerEnum.RemainingDaySearch.Month_4)
                {
                    targetTypeDate = "month";
                    differentDay = 5;
                }
                else if (request.RemainingDays == (int)CustomerEnum.RemainingDaySearch.All)
                {
                    targetTypeDate = "month";
                    differentDay = 6;
                }
                else if (request.RemainingDays == (int)CustomerEnum.RemainingDaySearch.Month_12)
                {
                    targetTypeDate = "month";
                    differentDay = 12;
                }
                else
                {
                    differentDay = request.RemainingDays ?? 0;
                }

                conditions.Add($"{SqlTables.StoreServices}.Active = 1 AND  DATEDIFF({targetTypeDate}, {SqlTables.StoreServices}.RenewDate,CONVERT(date ,GETUTCDATE())) <= {differentDay} ");
            }

        }

        private void PopupateCountSummariesAsync(CustomerSearchResponse response, GridReader reader)
        {
            //if (response.Summary == null)
            //    response.Summary = new SalesLeadSearchSummary();
        }

        private void PopulateMerchantType(CustomerSearchCondition request, List<string> extendJoins, List<string> extendJoinKeys, List<string> conditions, DynamicParameters parameters)
        {
            //if (request.Type != null && request.Type.Any()) // store in house or merchant
            //{
            //    conditions.Add($"{Alias}.Type = '{request.Type}'");
            //}

            if (request.Type == CustomerEnum.CustomerType.STORE_OF_MERCHANT.ToString())
                conditions.Add(SqlScript.Customer.Conditions.IsStoreOfMerchant);
            else if (request.Type == CustomerEnum.CustomerType.STORE_IN_HOUSE.ToString())
                conditions.Add(SqlScript.Customer.Conditions.IsStoreInHouse);
        }

        private void PopulateSummaryByMerchantType(CustomerSearchRequest request, List<string> extendJoins, List<string> extendJoinKeys, List<string> conditions, DynamicParameters parameters)
        {
            PopulateSummaryTotalStoreInHouse(request, extendJoins, extendJoinKeys, conditions, parameters);
            PopulateSummaryTotalStoreOfMerchant(request, extendJoins, extendJoinKeys, conditions, parameters);
        }

        private void PopulateSummaryTotalStoreInHouse(CustomerSearchRequest request, List<string> extendJoins, List<string> extendJoinKeys, List<string> conditions, DynamicParameters parameters)
        {
            var c_extendJoins = new List<string>(extendJoins);
            var c_conditions = new List<string>(conditions);
            var c_extendJoinKeys = new List<string>(extendJoinKeys);
            c_conditions.Add(SqlScript.Customer.Conditions.IsStoreInHouse);
            if (extendJoinKeys.Count > 0)
            {
                c_extendJoinKeys = c_extendJoinKeys.Distinct().ToList();
                c_extendJoins.AddRange(SqlScript.GetJoins(SqlScript.Customer.JoinsSearch, c_extendJoinKeys));
            }
            var parserQuery = new SqlParserParameter()
            {
                ExtendJoins = c_extendJoins,
                Conditions = c_conditions,
                FilterResultByColumnConditions = new List<string>(),
                QueryParam = request.SqlQueryParam,
                QueryTemplate = SqlScript.QuerySearch(Alias)
            };
            var sqlInfo = ParseSqlQueryInfoByTemplateCTE(parserQuery);
            request.SummaryQuery.QueryTotalStoreInHouse = sqlInfo.QueryTotalRecords;
        }

        private void PopulateSummaryTotalStoreOfMerchant(CustomerSearchRequest request, List<string> extendJoins, List<string> extendJoinKeys, List<string> conditions, DynamicParameters parameters)
        {
            var c_extendJoins = new List<string>(extendJoins);
            var c_conditions = new List<string>(conditions);
            var c_extendJoinKeys = new List<string>(extendJoinKeys);
            c_conditions.Add(SqlScript.Customer.Conditions.IsStoreOfMerchant);
            if (extendJoinKeys.Count > 0)
            {
                c_extendJoinKeys = c_extendJoinKeys.Distinct().ToList();
                c_extendJoins.AddRange(SqlScript.GetJoins(SqlScript.Customer.JoinsSearch, c_extendJoinKeys));
            }
            var parserQuery = new SqlParserParameter()
            {
                ExtendJoins = c_extendJoins,
                Conditions = c_conditions,
                FilterResultByColumnConditions = new List<string>(),
                QueryParam = request.SqlQueryParam,
                QueryTemplate = SqlScript.QuerySearch(Alias)
            };
            var sqlInfo = ParseSqlQueryInfoByTemplateCTE(parserQuery);
            request.SummaryQuery.QueryTotalStoreOfMerchant = sqlInfo.QueryTotalRecords;
        }

    }
}