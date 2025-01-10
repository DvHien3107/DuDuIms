using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class SalesLeadEntityMapper : DommelEntityMap<SalesLead>
    {
        public SalesLeadEntityMapper()
        {
            ToTable(SqlTables.SalesLead);
            Map(p => p.Id).IsKey();
            Map(p => p.FeaturesInteres).ToColumn(SqlColumns.SalesLead.FeaturesInteres);
            Map(p => p.FeaturesInteresOther).ToColumn(SqlColumns.SalesLead.FeaturesInteresOther);
            Map(p => p.InteractionStatus).ToColumn(SqlColumns.SalesLead.InteractionStatus);
            Map(p => p.SalonAddress).ToColumn(SqlColumns.SalesLead.Address);
            Map(p => p.City).ToColumn(SqlColumns.SalesLead.City);
            Map(p => p.Zipcode).ToColumn(SqlColumns.SalesLead.Zipcode);
            Map(p => p.SalonTimeZoneNumber).ToColumn(SqlColumns.SalesLead.SalonTimeZoneNumber);
            Map(p => p.Status).ToColumn(SqlColumns.SalesLead.Status);
            Map(p => p.StatusName).ToColumn(SqlColumns.SalesLead.StatusName);
            Map(p => p.SalonPhone).ToColumn(SqlColumns.SalesLead.Phone);
            Map(p => p.Product).ToColumn(SqlColumns.SalesLead.Product);
            Map(p => p.SalonName).ToColumn(SqlColumns.SalesLead.SalonName);
            Map(p => p.State).ToColumn(SqlColumns.SalesLead.State);
            Map(p => p.Type).ToColumn(SqlColumns.SalesLead.Type);
            Map(p => p.Version).ToColumn(SqlColumns.SalesLead.Version);
            Map(p => p.IsSendMail).ToColumn(SqlColumns.SalesLead.IsSendMail);
            Map(p => p.IsVerify).ToColumn(SqlColumns.SalesLead.IsVerify);
            Map(p => p.LicenseCode).ToColumn(SqlColumns.SalesLead.LicenseCode);
            Map(p => p.LicenseName).ToColumn(SqlColumns.SalesLead.LicenseName);
            Map(p => p.MoreInfo).ToColumn(SqlColumns.SalesLead.MoreInfo);
            Map(p => p.Password).ToColumn(SqlColumns.SalesLead.Password);
            Map(p => p.ContactName).ToColumn(SqlColumns.SalesLead.ContactName);
            Map(p => p.ContactPhone).ToColumn(SqlColumns.SalesLead.ContactPhone);
            Map(p => p.Country).ToColumn(SqlColumns.SalesLead.Country);
            Map(p => p.CreateTrialAt).ToColumn(SqlColumns.SalesLead.CreateTrialAt);
            Map(p => p.CreateTrialBy).ToColumn(SqlColumns.SalesLead.CreateTrialBy);
            Map(p => p.SalonEmail).ToColumn(SqlColumns.SalesLead.Email);
            Map(p => p.InteractionStatusId).ToColumn(SqlColumns.SalesLead.InteractionStatusId);
        }
    }
}
