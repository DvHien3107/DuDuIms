using Enrich.Dto.Attributes;
using Enrich.Dto.Base;
using System;
using System.Globalization;

namespace Enrich.IMS.Dto.NewCustomerGoal
{
    public partial class NewCustomerGoalListItemDto : ListItemDto
    {
        [GridField(Index = 1, ColumnName = "Id", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.NewCustomerGoal}.Id")]
        public string Id { get; set; }

        [GridField(Index = 1, ColumnName = "Year", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.NewCustomerGoal}.[Year]")]
        public int Year { get; set; }

        [GridField(Index = 1, ColumnName = "Month", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.NewCustomerGoal}.[Month]")]
        public int Month { get; set; }

        [GridField(Index = 1, ColumnName = "Goal", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.NewCustomerGoal}.[Goal]")]
        public int Goal { get; set; }

        [GridField(Index = 1, ColumnName = "Note", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.NewCustomerGoal}.[Note]")]
        public string Note { get; set; }

        [GridField(Index = 1, ColumnName = "CreatedBy", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.NewCustomerGoal}.[CreatedBy]")]
        public string CreatedBy { get; set; }

        [GridField(Index = 1, ColumnName = "UpdatedBy", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.NewCustomerGoal}.[UpdatedBy]")]
        public string UpdatedBy { get; set; }

        [GridField(Index = 1, ColumnName = "CreatedDate", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.NewCustomerGoal}.[CreatedDate]")]
        public DateTime? CreatedDate { get; set; }

        [GridField(Index = 1, ColumnName = "UpdatedDate", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.NewCustomerGoal}.[UpdatedDate]")]
        public DateTime? UpdatedDate { get; set; }

        [GridField(Index = 1, ColumnName = "PartnerName", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Partner}.[Name]")]
        public string PartnerName { get; set; }

        [GridField(Index = 1, ColumnName = "UpdatedDate", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Partner}.[Code]")]
        public string PartnerCode { get; set; }

        [GridField(Index = 1, ColumnName = "SiteId", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.NewCustomerGoal}.[SiteId]")]
        public int SiteId { get; set; }

    }
}
