namespace Enrich.IMS.Entities
{
    public partial class DejavoDatasheet
    {
        public long Id { get; set; }
        public string OrdersCode { get; set; }
        public string M_Name { get; set; }
        public string M_Address { get; set; }
        public string M_State { get; set; }
        public string M_City { get; set; }
        public string M_ZipCode { get; set; }
        public string M_Country { get; set; }
        public string M_Telephone { get; set; }
        public string Software { get; set; }
        public string AcquirerBin { get; set; }
        public string POS_MerchantNumber { get; set; }
        public string TerminalID { get; set; }
        public string StoreNumber { get; set; }
        public string TerminalNumber { get; set; }
        public string M_LocationNumber { get; set; }
        public string M_CategoryCode { get; set; }
        public string AgentNumber { get; set; }
        public string ChainNumber { get; set; }
        public string TimeZoneDifference { get; set; }
        public string ABA { get; set; }
        public string FIID { get; set; }
        public string SharingGroup { get; set; }
        public string RemibursementAttribute { get; set; }
        public string Authorization { get; set; }
        public string Settlement { get; set; }
        public string Voice_Authorization { get; set; }
        public string URL { get; set; }
        public string PORT { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string CreateBy { get; set; }
        public string UpdateBy { get; set; }
        public string PDF_Url { get; set; }
    }
}
