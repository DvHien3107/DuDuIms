namespace Enrich.IMS.Entities
{    
    public partial class QuestionareNonTechnician
    {
        public string Id { get; set; }
        public string QuestionareId { get; set; }
        public string Name { get; set; }
        public string Job { get; set; }
        public string Pay { get; set; }
        public bool? AdjustViewPayroll { get; set; }
        public System.DateTime? CreateAt { get; set; }
    }
}
