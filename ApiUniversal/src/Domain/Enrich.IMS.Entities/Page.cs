namespace Enrich.IMS.Entities
{    
    public partial class Page
    {
        public string PageCode { get; set; }
        public string PageName { get; set; }
        public string Description { get; set; }
        public int? Order { get; set; }
        public string Link { get; set; }
    }
}
