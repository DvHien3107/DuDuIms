namespace Enrich.IMS.Entities
{
    public partial class LicenseItem
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public long? GroupID { get; set; }
        public string GroupName { get; set; }
        public string Code { get; set; }
        public bool? Enable { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool? BuiltIn { get; set; }
    }
}
