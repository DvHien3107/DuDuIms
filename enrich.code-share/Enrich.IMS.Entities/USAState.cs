namespace Enrich.IMS.Entities
{
    public partial class USAState
    {
        public string abbreviation { get; set; }
        public string name { get; set; }
        public string country { get; set; }
        public string type { get; set; }
        public int? sort { get; set; }
        public string status { get; set; }
        public string occupied { get; set; }
        public string notes { get; set; }
        public string fips_state { get; set; }
        public string assoc_press { get; set; }
        public string standard_federal_region { get; set; }
        public decimal?sales_tax { get; set; }
    }
}
