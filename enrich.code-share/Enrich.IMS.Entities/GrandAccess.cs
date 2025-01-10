namespace Enrich.IMS.Entities
{
    public partial class GrandAccess
    {
        public string FunctionCode { get; set; }
        public string FunctionName { get; set; }
        public string RoleCode { get; set; }
        public string RoleName { get; set; }
        public bool? Access { get; set; }
    }
}
