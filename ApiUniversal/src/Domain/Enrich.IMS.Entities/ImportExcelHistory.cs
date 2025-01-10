namespace Enrich.IMS.Entities
{
    public partial class ImportExcelHistory
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public System.DateTime? ImportAt { get; set; }
        public string ImportFile { get; set; }
    }
}
