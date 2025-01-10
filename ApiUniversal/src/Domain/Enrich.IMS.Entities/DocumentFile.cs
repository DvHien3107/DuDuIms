namespace Enrich.IMS.Entities
{
    public partial class DocumentFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Link { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public string UpdateBy { get; set; }
        public string UploadIds { get; set; }
    }
}
