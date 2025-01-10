namespace Enrich.IMS.Entities
{
    public partial class QuestionareTechnician
    {
        public string Id { get; set; }
        public string QuestionareId { get; set; }
        public string Name { get; set; }
        public string NickName { get; set; }
        public string Commission { get; set; }
        public string PayrollSplitCheckOrCash { get; set; }
        public bool? AddDiscounts { get; set; }
        public bool? AdjustPrices { get; set; }
        public System.DateTime? CreateAt { get; set; }
    }
}
