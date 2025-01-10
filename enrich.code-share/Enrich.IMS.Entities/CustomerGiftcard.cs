using System.ComponentModel.DataAnnotations.Schema;

namespace Enrich.IMS.Entities
{
    public partial class CustomerGiftcard
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public string OrderCode { get; set; }
        public string CustomerCode { get; set; }
        public string QRCode { get; set; }
        public string Printed { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string CreateBy { get; set; }
        public string StoreServiceId { get; set; }
    }
}
