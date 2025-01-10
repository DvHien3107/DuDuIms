namespace Enrich.IMS.Dto
{
    public partial class CalendarEventDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public string TicketId { get; set; }
        public string TicketName { get; set; }
        public string GMT { get; set; }
        public string TimeZone { get; set; }
        public string StartEvent { get; set; }
        public string EndEvent { get; set; }
        public int? Done { get; set; }
        public System.DateTime? LastUpdateAt { get; set; }
        public string LastUpdateBy { get; set; }
        public string LastUpdateByNumber { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string CreateBy { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string MemberNumber { get; set; }
        public string Type { get; set; }
        public string SalesLeadId { get; set; }
        public string SalesLeadName { get; set; }
        public int? DemoSchedulerId { get; set; }
        public string Location { get; set; }
        public string AttendeesNumber { get; set; }
        public string AttendeesName { get; set; }
        public string GoogleCalendarId { get; set; }
        public string MemberName { get; set; }
        public System.DateTime? StartUTC { get; set; }
        public System.DateTime? EndUTC { get; set; }
        public string ETag { get; set; }
        public string REF { get; set; }
        public bool? Status { get; set; }
    }
}
