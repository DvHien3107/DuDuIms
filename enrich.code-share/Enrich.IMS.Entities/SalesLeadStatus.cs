namespace Enrich.IMS.Entities
{
    
    public partial class SalesLeadStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Order { get; set; }
        public bool? PendingCloseContract { get; set; }
        public string Color { get; set; }
    }
}
