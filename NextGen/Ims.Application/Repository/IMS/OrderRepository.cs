using Dapper;
using Pos.Application.Extensions.Helper;
using Pos.Model.Enum.IMS;
using Pos.Model.Model.Table.IMS;
using Promotion.Application.Extensions;
using Promotion.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Repository.IMS
{
    public interface IOrderRepository : IEntityService<O_Orders>
    {
        Task InsertNewOrder(O_Orders order);
        Task<O_Orders> getOrderByCode(string orderCode);
        Task InsertOrderSubcription(Order_Subcription order);
        Task<License_Product> getLicenseProductByCode(string Code);
        Task<List<Order_Subcription>> getOrderSubcriptionByCode(string orderCode);
        Task updateOrderStauts(OrderStatus status, string OrdersCode, string PaymentMethod, string PaymentNote);
        Task LoadOrder(string orderCode);
        Task<List<O_Orders>> getPendingLst();
        Task<List<Order_Subcription>> getListSubcription(string OrderCode);
        Task<List<License_Product_Item>> getListLicenseCode(string LicenseProductCode);
        Task CloseOrder(string OrderCode);
        Task<C_CustomerTransaction> getCusTransByCode(string OrderCode);
        Task<O_Orders> checkOrderToClose(string OrderCode);
    }
    public class OrderRepository : IMSEntityService<O_Orders>, IOrderRepository
    {
        public async Task<O_Orders> checkOrderToClose(string OrderCode)
        {
            return await _connection.QueryFirstOrDefaultAsync<O_Orders>("exec P_CheckOrderToClose @OrderCode", new { OrderCode });
        }
        public async Task<List<O_Orders>> getPendingLst()
        {
            var result = await _connection.SqlQueryAsync<O_Orders>("exec P_GetPendingLst");
            return result.ToList();
        }
        public async Task<O_Orders> getOrderByCode(string OrderCode)
        {
            return await _connection.SqlFirstOrDefaultAsync<O_Orders>("select * from O_Orders with (nolock) where OrdersCode = @OrderCode", new { OrderCode });
        }
        public async Task<C_CustomerTransaction> getCusTransByCode(string OrderCode)
        {
            return await _connection.SqlFirstOrDefaultAsync<C_CustomerTransaction>("select * from C_CustomerTransaction with (nolock) where OrdersCode =@OrderCode", new { OrderCode });
        }
        public async Task<List<Order_Subcription>> getListSubcription(string OrderCode)
        {
            var result = await _connection.SqlQueryAsync<Order_Subcription>("select * from Order_Subcription with (nolock) where OrderCode = @OrderCode and SubscriptionType in ('license', 'addon')", new { OrderCode });
            return result.ToList();
        }

        public async Task<List<License_Product_Item>> getListLicenseCode(string LicenseProductCode)
        {
            var result = await _connection.SqlQueryAsync<License_Product_Item>("select * from License_Product_Item with (nolock) where License_Product_Id in (select id from License_Product with (nolock) where Code=@LicenseProductCode)", new { LicenseProductCode });
            return result.ToList();
        }

        public async Task CloseOrder(string OrderCode)
        {
            await _connection.ExecuteAsync("update O_Orders set Status='Closed' where Status='Paid_Wait' and OrdersCode = @OrderCode", new { OrderCode });
        }
        public async Task LoadOrder(string orderCode)
        {
            await _connection.ExecuteAsync("exec  P_LoadOrder @orderCode", new { orderCode });
        }

        public async Task updateOrderStauts(OrderStatus status, string OrdersCode, string PaymentMethod, string PaymentNote)
        {
            await _connection.ExecuteAsync("update O_Orders set Status = @Status, PaymentMethod =@PaymentMethod, PaymentNote =@PaymentNote where OrdersCode=@OrdersCode",new { Status = status.ToString(), OrdersCode, PaymentMethod, PaymentNote });
        }

        public async Task InsertNewOrder(O_Orders order)
        {
            await _connection.InsertAsync(order);
        }
        
        public async Task InsertOrderSubcription(Order_Subcription order)
        {
            await _connection.InsertAsync(order);
        }

        public async Task<List<Order_Subcription>> getOrderSubcriptionByCode(string orderCode)
        {
           return (await _connection.SqlQueryAsync<Order_Subcription>("select * from Order_Subcription with (nolock) where OrderCode = @orderCode", new { orderCode })).ToList();
        }
        public async Task<License_Product> getLicenseProductByCode(string Code)
        {
            return await _connection.SqlFirstOrDefaultAsync<License_Product>("select * from License_Product with (nolock) where Code = @Code", new { Code });
        }

        
    }
}
