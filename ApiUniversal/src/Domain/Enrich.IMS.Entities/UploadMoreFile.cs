namespace Enrich.IMS.Entities
{   
    public partial class UploadMoreFile
    {
        public long UploadId { get; set; }
        public long? TableId { get; set; }
        public string TableName { get; set; }
        public string FileName { get; set; }
    }
}
