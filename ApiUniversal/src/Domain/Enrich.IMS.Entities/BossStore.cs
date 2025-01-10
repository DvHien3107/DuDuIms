
namespace Enrich.IMS.Entities
{
    public partial class BossStore
    {
        public string Id { get; set; }
        public string Owner { get; set; }
        public string ContactPerson { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string IdBossManage { get; set; }
        public string UrlConnect { get; set; }
        public string StoreCodes { get; set; }
        public string Description { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public string UpdateBy { get; set; }
        public string Other { get; set; }
        public bool? IsActive { get; set; }
    }
}
