using Enrich.Dto.Attributes;
using Enrich.Dto.Base;

namespace Enrich.IMS.Dto.TicketFeedback
{
    public partial class TicketFeedbackItemDto : ListItemDto
    {
        [GridField(Index = 1, ColumnName = "Id", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.TicketFeedback + ".Id")]
        public string Id { get; set; }

        [GridField(Index = 2, ColumnName = "Title", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.TicketFeedback + ".Title")]
        public string Title { get; set; }

        [GridField(Index = 3, ColumnName = "Description", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.TicketFeedback + ".Description")]
        public string Description { get; set; }

        [GridField(Index = 5, ColumnName = "SalesLeadId", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.TicketFeedback + ".SalesLeadId")]
        public string SalesLeadId { get; set; }

        [GridField(Index = 6, ColumnName = "CreateAt", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.TicketFeedback + ".CreateAt")]
        public string CreateAt { get; set; }

        [GridField(Index = 7, ColumnName = "CreateBy", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.TicketFeedback + ".CreateBy")]
        public string CreateBy { get; set; }

        [GridField(Index = 8, ColumnName = "UpdateAt", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.TicketFeedback + ".UpdateAt")]
        public string UpdateAt { get; set; }

        [GridField(Index = 9, ColumnName = "UpdateBy", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(SqlTables.TicketFeedback + ".UpdateBy")]
        public string UpdateBy { get; set; }

        [GridField(Index = 9, ColumnName = "CreateByName", IsDefault = true, IsShow = true, CanSort = true)]
        public string CreateByName { get; set; }
    }
}
