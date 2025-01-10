using Enrich.Dto.Attributes;
using Enrich.Dto.Base;

namespace Enrich.IMS.Dto.SalesLead
{
    public partial class SalesLeadListItemDto : ListItemDto
    {
        [GridField(Index = 1, ColumnName = "Id", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.SalesLead + ".Id")]
        public string Id { get; set; }

        [GridField(Index = 2, ColumnName = "StatusName", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]       
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.StatusName}")]
        public string StatusName { get; set; }

        [GridField(Index = 3, ColumnName = "Rate", IsDefault = true, IsShow = false, CanSort = true)]
        [SqlMapDto(SqlTables.SalesLead + ".PotentialRateScore")]
        public int? PotentialRateScore { get; set; }

        [GridField(Index = 4, ColumnName = "MemberNumber", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto(SqlTables.SalesLead + ".MemberNumber")]
        public string SaleMemberNumber { get; set; }

        [GridField(Index = 5, ColumnName = "CustomerCode", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto(SqlTables.SalesLead + ".CustomerCode")]
        public string CustomerCode { get; set; }

        // dont use
        //[GridField(Index = 6, IsDefault = true, IsShow = true, CanSort = true)]
        //[SqlMapDto(SqlTables.SalesLead + ".CustomerName")]
        //public string CustomerName { get; set; }

        [GridField(Index = 7, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.SalesLead + ".CreateAt")]
        public System.DateTime? CreateAt { get; set; }

        [GridField(Index = 8, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.SalesLead + ".CreateBy")]
        public string CreateBy { get; set; }

        [GridField(Index = 9, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.SalesLead + ".UpdateAt")]
        public System.DateTime? UpdateAt { get; set; }

        [GridField(Index = 10, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.SalesLead + ".UpdateBy")]
        public string UpdateBy { get; set; }

        [GridField(Index = 11, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.SalonName}")]
        public string SalonName { get; set; }

        [GridField(Index = 12, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.Address}")]
        public string Address { get; set; }

        [GridField(Index = 13, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.State}")]
        public string State { get; set; }

        [GridField(Index = 14, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.City}")]
        public string City { get; set; }

        [GridField(Index = 15, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.Zipcode}")]
        public string Zipcode { get; set; }

        [GridField(Index = 16, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.Country}")]
        public string Country { get; set; }

        [GridField(Index = 17, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.Phone}")]
        public string Phone { get; set; }

        [GridField(Index = 18, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.ContactName}")]
        public string ContactName { get; set; }

        [GridField(Index = 19, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.ContactPhone}")]
        public string ContactPhone { get; set; }

        [GridField(Index = 20, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.MoreInfo}")]
        public string MoreInfo { get; set; }

        [GridField(Index = 21, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.Type}")]
        public string RegisterType { get; set; }

        [GridField(Index = 22, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.Email}")]
        public string Email { get; set; }

        [GridField(Index = 23, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.IsVerify}")]
        public bool? IsVerify { get; set; }

        [GridField(Index = 24, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.IsSendMail}")]
        public bool? IsSendMail { get; set; }

        // dont use
        //[GridField(Index = 25, IsDefault = true, IsShow = true, CanSort = true)]
        //[SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.CreateTrialBy}")]
        //public string CreateTrialBy { get; set; }

        // dont use
        //[GridField(Index = 26, IsDefault = true, IsShow = true, CanSort = true)]
        //[SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.CreateTrialAt}")]
        //public System.DateTime? CreateTrialAt { get; set; }

        // dont use
        //[GridField(Index = 27, IsDefault = true, IsShow = true, CanSort = true)]
        //[SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.Product}")]
        //public string Product { get; set; }

        // dont use
        //[GridField(Index = 28, IsDefault = true, IsShow = true, CanSort = true)]
        //[SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.Version}")]
        //public string Version { get; set; }

        [GridField(Index = 29, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.SalesLead + ".SalonTimeZone")]
        public string SalonTimeZone { get; set; }

        [GridField(Index = 30, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.SalesLead + ".SalonTimeZone_Number")]
        public string SalonTimeZoneNumber { get; set; }

        [GridField(Index = 31, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.SalesLead + ".TeamNumber")]
        public long? TeamNumber { get; set; }

        [GridField(Index = 32, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.InteractionStatus}")]
        public string InteractionStatus { get; set; }

        [GridField(Index = 33, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.SalesLead + ".CallOfNumber")]
        public string CallOfNumber { get; set; }

        [GridField(Index = 34, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.SalesLead + ".MemberName")]
        public string SaleMemberName { get; set; }

        [GridField(Index = 35, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.SalesLead + ".TeamName")]
        public string SaleTeamName { get; set; }

        [GridField(Index = 36, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.LicenseName}")]
        public string LicenseName { get; set; }

        [GridField(Index = 37, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.LicenseCode}")]
        public string LicenseCode { get; set; }

        [GridField(Index = 38, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.SalesLead + ".LastUpdateDesc")]
        public string LastUpdate { get; set; }

        [GridField(Index = 39, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.SalesLead + ".LastNoteAt")]
        public System.DateTime? LastNoteAt { get; set; }

        [GridField(Index = 40, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.SalesLead + ".LastNote")]
        public string LastNote { get; set; }

        [GridField(Index = 41, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.SalesLead}.{SqlColumns.SalesLead.InteractionStatusId}")]
        public int? InteractionStatusId { get; set; }

        [GridField(Index = 42, IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.SalesLead + ".CreateByMemberNumber")]
        public string CreateByMemberNumber { get; set; }
    }
}
