namespace Enrich.IMS.Entities
{
    public partial class MerchantSubscribe
    {
        public long Id { get; set; }
        public string CustomerCode { get; set; }
        public string ApplicationId { get; set; }
        public string Document_ApplicationAndAgreement { get; set; }
        public string Document_ProcessingStatements { get; set; }
        public string Document_ProofOfBusiness { get; set; }
        public string Document_VoidCheck { get; set; }
        public string Document_EquipmentForm { get; set; }
        public string Document_NonProfitEvidence { get; set; }
        public string Document_Other { get; set; }
        public string MerchantID { get; set; }
        public string DbaName { get; set; }
        public string GatewayMerchantId { get; set; }
        public string GatewayTerminalNumbers { get; set; }
        public string SharedSecret { get; set; }
        public string MessageType { get; set; }
        public string UId { get; set; }
        public string ResponseStatus { get; set; }
        public string ResponseCodeFromNuvei { get; set; }
        public long? TicketId { get; set; }
        public string StatementDescriptor { get; set; }
        public string Agent { get; set; }
        public string Office { get; set; }
        public System.DateTime? StartDate { get; set; }
        public System.DateTime? EndDate { get; set; }
        public string CardTypeAccept { get; set; }
        public string Status { get; set; }
    }
}
