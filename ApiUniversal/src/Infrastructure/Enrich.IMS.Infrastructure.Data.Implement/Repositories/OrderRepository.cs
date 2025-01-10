using Dapper;
using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Core.Container;
using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public class OrderRepository : DapperGenericRepository<Order>, IOrderRepository
    {
        private readonly EnrichContext _context;
        private readonly IEnrichContainer _container;
        public OrderRepository(
            IConnectionFactory connectionFactory,
            EnrichContext context,
            IEnrichContainer container)
            : base(connectionFactory)
        {
            _context = context;
            _container = container;
        }
        public Order GetByOrderCode(string orderCode)
        {
            var query = $"SELECT TOP 1 * FROM {SqlTables.Orders} WITH (NOLOCK) WHERE OrdersCode = @ordercode";
            var parameters = new DynamicParameters();
            parameters.Add("ordercode", orderCode);
            using (var connection = GetDbConnection())
            {
                return connection.QueryFirstOrDefault<Order>(query, parameters);
            }
        }
        public async Task<Order> GetByOrderCodeAsync(string orderCode)
        {
            var query = $"SELECT TOP 1 * FROM {SqlTables.Orders} WITH (NOLOCK) WHERE OrdersCode = @ordercode";
            var parameters = new DynamicParameters();
            parameters.Add("ordercode", orderCode);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Order>(query, parameters);
            }
        }
        public async Task<IEnumerable<OrderDto>> GetPaymentLaterByDateAsync(DateTime fromDate)
        {
            var query = SqlScript.Order.GetPaymentLaterByDate(fromDate);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<OrderDto>(query);
            }
        }
        public int GetNumberOrderOnDate()
        {
            var query = $"SELECT COUNT(*) FROM {SqlTables.Orders} WITH (NOLOCK) WHERE YEAR(CreatedAt) = YEAR(GETUTCDATE()) AND MONTH(CreatedAt) = MONTH(GETUTCDATE())";
            using (var connection = GetDbConnection())
            {
                var number = connection.QueryFirstOrDefault<int>(query);
                return number;
            }
        }
        public async Task<int> GetNumberOrderOnDateAsync()
        {
            var query = $"SELECT COUNT(*) FROM {SqlTables.Orders} WITH (NOLOCK) WHERE YEAR(CreatedAt) = YEAR(GETUTCDATE()) AND MONTH(CreatedAt) = MONTH(GETUTCDATE())";
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<int>(query);
            }
        }
        public async Task<IEnumerable<OrderSubscription>> GetOrderSubscriptions(string OrderCode)
        {
            var query = $"SELECT * FROM {SqlTables.OrderSubscription} WHERE OrderCode = @ordercode";
            var parameters = new DynamicParameters();
            parameters.Add("ordercode", OrderCode);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<OrderSubscription>(query, parameters);
            }
        }
        public async Task<bool> CreateAsync(OrderDto order, List<OrderSubscriptionDto> orderSubscriptions)
        {
            var query = @"INSERT INTO [O_Orders] ([Id],[OrdersCode],[CustomerCode],[CustomerName],[SalesMemberNumber],[SalesName],[GrandTotal],[Comment],[Approved],[ApprovedBy],
                            [ApprovedAt],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedHistory],[Status],[TotalHardware_Amount],[ShippingFee],[Service_Amount],[DiscountAmount],[DiscountPercent],
                            [IsDelete],[ShippingDate],[OtherFee],[TaxRate],[CreateByMemNumber],[ShipVIA],[ShippingAddress],[BundelStatus],[StatusHistory],[Cancel],[Note_Packaging],
                            [Note_Delivery],[InvoiceNumber],[DeploymentStatus],[StartDate],[UpdatedBy],[DueDate],[InvoiceDate],[Renewal],[PaymentMethod],[PaymentNote],[PartnerCode],[CreateDeployTicket])
                         VALUES (@Id,@OrdersCode,@CustomerCode,@CustomerName,@SalesMemberNumber,@SalesName,@GrandTotal,@Comment,@Approved,@ApprovedBy,@ApprovedAt,@CreatedAt,@CreatedBy
                            ,@UpdatedAt,@UpdatedHistory,@Status,@TotalHardware_Amount,@ShippingFee,@Service_Amount,@DiscountAmount,@DiscountPercent,@IsDelete,@ShippingDate,@OtherFee,@TaxRate
                            ,@CreateByMemNumber,@ShipVIA,@ShippingAddress,@BundelStatus,@StatusHistory,@Cancel,@Note_Packaging,@Note_Delivery,@InvoiceNumber,@DeploymentStatus,@StartDate
                            ,@UpdatedBy,@DueDate,@InvoiceDate,@Renewal,@PaymentMethod,@PaymentNote,@PartnerCode,@CreateDeployTicket)";
            var parameters = new DynamicParameters();
            parameters.Add("Id", order.Id);
            parameters.Add("OrdersCode", order.OrdersCode);
            parameters.Add("CustomerCode", order.CustomerCode);
            parameters.Add("CustomerName", order.CustomerName);
            parameters.Add("SalesMemberNumber", order.SalesMemberNumber);
            parameters.Add("SalesName", order.SalesName);
            parameters.Add("GrandTotal", order.GrandTotal);
            parameters.Add("Comment", order.Comment);
            parameters.Add("Approved", order.Approved);
            parameters.Add("ApprovedBy", order.ApprovedBy);
            parameters.Add("ApprovedAt", order.ApprovedAt);
            parameters.Add("CreatedAt", order.CreatedAt);
            parameters.Add("CreatedBy", order.CreatedBy);
            parameters.Add("UpdatedAt", order.UpdatedAt);
            parameters.Add("UpdatedHistory", order.UpdatedHistory);
            parameters.Add("Status", order.Status);
            parameters.Add("TotalHardware_Amount", order.TotalHardwareAmount);
            parameters.Add("ShippingFee", order.ShippingFee);
            parameters.Add("Service_Amount", order.ServiceAmount);
            parameters.Add("DiscountAmount", order.DiscountAmount);
            parameters.Add("DiscountPercent", order.DiscountPercent);
            parameters.Add("IsDelete", order.IsDelete);
            parameters.Add("ShippingDate", order.ShippingDate);
            parameters.Add("OtherFee", order.OtherFee);
            parameters.Add("TaxRate", order.TaxRate);
            parameters.Add("CreateByMemNumber", order.CreateByMemNumber);
            parameters.Add("ShipVIA", order.ShipVIA);
            parameters.Add("ShippingAddress", order.ShippingAddress);
            parameters.Add("BundelStatus", order.BundelStatus);
            parameters.Add("StatusHistory", order.StatusHistory);
            parameters.Add("Cancel", order.Cancel);
            parameters.Add("Note_Packaging", order.NotePackaging);
            parameters.Add("Note_Delivery", order.NoteDelivery);
            parameters.Add("InvoiceNumber", order.InvoiceNumber);
            parameters.Add("DeploymentStatus", order.DeploymentStatus);
            parameters.Add("StartDate", order.StartDate);
            parameters.Add("UpdatedBy", order.UpdatedBy);
            parameters.Add("DueDate", order.DueDate);
            parameters.Add("InvoiceDate", order.InvoiceDate);
            parameters.Add("Renewal", order.Renewal);
            parameters.Add("PaymentMethod", order.PaymentMethod);
            parameters.Add("PaymentNote", order.PaymentNote);
            parameters.Add("PartnerCode", order.PartnerCode);
            parameters.Add("CreateDeployTicket", order.CreateDeployTicket);

            var query_os = @"INSERT INTO [Order_Subcription]
                            ([Id]
                            ,[StoreCode]
                            ,[OrderCode]
                            ,[ProductId]
                            ,[ProductName]
                            ,[CustomerCode]
                            ,[CustomerName]
                            ,[Price]
                            ,[Period]
                            ,[PurcharsedDay]
                            ,[NumberOfItem]
                            ,[AutoRenew]
                            ,[StartDate]
                            ,[Actived]
                            ,[IsAddon]
                            ,[EndDate]
                            ,[Product_Code]
                            ,[Product_Code_POSSystem]
                            ,[Quantity]
                            ,[Discount]
                            ,[DiscountPercent]
                            ,[PriceType]
                            ,[Promotion_Apply_Months]
                            ,[SubscriptionType]
                            ,[ApplyDiscountAsRecurring]
                            ,[PeriodRecurring]
                            ,[SubscriptionQuantity]
                            ,[Amount]
                            ,[ApplyPaidDate]
                            ,[RecurringPrice])
                         VALUES
                            (@Id
                            ,@StoreCode
                            ,@OrderCode
                            ,@ProductId
                            ,@ProductName
                            ,@CustomerCode
                            ,@CustomerName
                            ,@Price
                            ,@Period
                            ,@PurcharsedDay
                            ,@NumberOfItem
                            ,@AutoRenew
                            ,@StartDate
                            ,@Actived
                            ,@IsAddon
                            ,@EndDate
                            ,@Product_Code
                            ,@Product_Code_POSSystem
                            ,@Quantity
                            ,@Discount
                            ,@DiscountPercent
                            ,@PriceType
                            ,@Promotion_Apply_Months
                            ,@SubscriptionType
                            ,@ApplyDiscountAsRecurring
                            ,@PeriodRecurring
                            ,@SubscriptionQuantity
                            ,@Amount
                            ,@ApplyPaidDate
                            ,@RecurringPrice)";
            var parameters_os = new List<DynamicParameters> { };
            Parallel.ForEach(orderSubscriptions, orderSubscription =>
            {
                var parameters_temp = new DynamicParameters();
                parameters_temp.Add("Id", orderSubscription.Id);
                parameters_temp.Add("StoreCode", orderSubscription.StoreCode);
                parameters_temp.Add("OrderCode", orderSubscription.OrderCode);
                parameters_temp.Add("ProductId", orderSubscription.ProductId);
                parameters_temp.Add("ProductName", orderSubscription.ProductName);
                parameters_temp.Add("CustomerCode", orderSubscription.CustomerCode);
                parameters_temp.Add("CustomerName", orderSubscription.CustomerName);
                parameters_temp.Add("Price", orderSubscription.Price);
                parameters_temp.Add("Period", orderSubscription.Period);
                parameters_temp.Add("PurcharsedDay", orderSubscription.PurcharsedDay);
                parameters_temp.Add("NumberOfItem", orderSubscription.NumberOfItem);
                parameters_temp.Add("AutoRenew", orderSubscription.AutoRenew);
                parameters_temp.Add("StartDate", orderSubscription.StartDate);
                parameters_temp.Add("Actived", orderSubscription.Actived);
                parameters_temp.Add("IsAddon", orderSubscription.IsAddon);
                parameters_temp.Add("EndDate", orderSubscription.EndDate);
                parameters_temp.Add("Product_Code", orderSubscription.ProductCode);
                parameters_temp.Add("Product_Code_POSSystem", orderSubscription.ProductCodePOSSystem);
                parameters_temp.Add("Quantity", orderSubscription.Quantity);
                parameters_temp.Add("Discount", orderSubscription.Discount);
                parameters_temp.Add("DiscountPercent", orderSubscription.DiscountPercent);
                parameters_temp.Add("PriceType", orderSubscription.PriceType);
                parameters_temp.Add("Promotion_Apply_Months", orderSubscription.PromotionApplyMonths);
                parameters_temp.Add("SubscriptionType", orderSubscription.SubscriptionType);
                parameters_temp.Add("ApplyDiscountAsRecurring", orderSubscription.ApplyDiscountAsRecurring);
                parameters_temp.Add("PeriodRecurring", orderSubscription.PeriodRecurring);
                parameters_temp.Add("SubscriptionQuantity", orderSubscription.SubscriptionQuantity);
                parameters_temp.Add("Amount", orderSubscription.Amount);
                parameters_temp.Add("ApplyPaidDate", orderSubscription.ApplyPaidDate);
                parameters_temp.Add("RecurringPrice", orderSubscription.RecurringPrice);
                parameters_os.Add(parameters_temp);
            });

            using (var connection = GetDbConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await connection.ExecuteAsync(query, parameters, transaction);
                        foreach (var param in parameters_os)
                        {
                            await connection.ExecuteAsync(query_os, param, transaction);
                        }
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }
        public async Task<bool> ChangeStatusAsync(string orderId, string status)
        {
            var order = await FindByIdAsync(orderId);
            if (order == null) throw new Exception(ValidationMessages.NotFound.Order);
            if (!EnumHelper.IsDefined<OrderEnum.Status>(status)) throw new Exception(ValidationMessages.NotDefined.OrderStatus);

            //if (status == order.Status) return true;
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            order.UpdatedBy = _context.UserFullName;
            order.UpdatedHistory += $"|{order.UpdatedAt?.ToString(Constants.Format.Date_MMMddyyyy_HHmm)} - By: {order.UpdatedBy}";
            order.StatusHistory += $"|{EnumHelper.DisplayName(EnumHelper.GetEnum<OrderEnum.Status>(status))} - Update by: {order.UpdatedBy} - At: {order.UpdatedAt?.ToString(Constants.Format.Date_MMMddyyyy_HHmm)}";
            await UpdateAsync(order);

            return true;
        }
    }
}