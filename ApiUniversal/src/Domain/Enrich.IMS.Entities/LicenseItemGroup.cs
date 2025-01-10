namespace Enrich.IMS.Entities
{
    public partial class LicenseItemGroup
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public long? ParentID { get; set; }
        public string ParentName { get; set; }
        public string Options { get; set; }
    }
}
