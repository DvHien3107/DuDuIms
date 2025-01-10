namespace Enrich.IMS.Entities
{
    public partial class ExternalEmbed
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FunctionCode { get; set; }
        public string EmbedScript { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string CreateBy { get; set; }
        public bool? Visible { get; set; }
    }
}
