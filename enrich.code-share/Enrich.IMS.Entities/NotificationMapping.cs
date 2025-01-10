namespace Enrich.IMS.Entities
{    
    public partial class NotificationMapping
    {
        public int Id { get; set; }
        public string MemberNumber { get; set; }
        public int NotificationId { get; set; }
        public bool? IsView { get; set; }
        public System.DateTime? ViewTime { get; set; }
        public bool? IsSent { get; set; }
        public System.DateTime? CreateAt { get; set; }
    
        public virtual Notification P_Notification { get; set; }
    }
}
