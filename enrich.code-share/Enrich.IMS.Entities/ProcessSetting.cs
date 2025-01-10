
namespace Enrich.IMS.Entities
{
    public partial class ProcessSetting
    {
        public long Id { get; set; }
        public string FieldName { get; set; }
        public bool? Requirement { get; set; }
        public string FieldType { get; set; }
        public bool? IsCheck { get; set; }
    }
}
