
namespace Enrich.IMS.Entities
{  
    public partial class DocumentCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        public int? DisplayOrder { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public string UpdateBy { get; set; }
    }
}
