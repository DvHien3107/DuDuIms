using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class StoreServiceEntityMapper : DommelEntityMap<StoreService>
    {
        public StoreServiceEntityMapper()
        {
            ToTable(SqlTables.StoreServices);
            Map(p => p.Id).IsKey();
            Map(p => p.HasRenewInvoiceIncomplete).ToColumn(SqlColumns.StoreServices.HasRenewInvoiceIncomplete);
            Map(p => p.MxMerchantCardAccountId).ToColumn(SqlColumns.StoreServices.MxMerchantCardAccountId);
            Map(p => p.MxMerchantContractId).ToColumn(SqlColumns.StoreServices.MxMerchantContractId);
            Map(p => p.MxMerchantSubscriptionId).ToColumn(SqlColumns.StoreServices.MxMerchantSubscriptionId);
            Map(p => p.MxMerchantSubscriptionStatus).ToColumn(SqlColumns.StoreServices.MxMerchantSubscriptionStatus);
            Map(p => p.ProductCodePOSSystem).ToColumn(SqlColumns.StoreServices.ProductCodePOSSystem);
        }
    }
}
