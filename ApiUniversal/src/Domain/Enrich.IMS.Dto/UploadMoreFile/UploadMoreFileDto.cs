namespace Enrich.IMS.Dto
{   
    public partial class UploadMoreFileDto
    {
        public long UploadId { get; set; }
        public long? TableId { get; set; }
        public string TableName { get; set; }
        public string FileName { get; set; }
    }
}
