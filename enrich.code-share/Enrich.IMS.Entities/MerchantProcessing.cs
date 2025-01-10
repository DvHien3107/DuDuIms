
namespace Enrich.IMS.Entities
{
    public partial class MerchantProcessing
    {
        public int Id { get; set; }
        public string TPN { get; set; }
        public string RegisterID { get; set; }
        public string Auth { get; set; }
        public string Note { get; set; }
        public System.DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public long? MerchantSubscribeId { get; set; }
    }
}
