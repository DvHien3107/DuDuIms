using System.ComponentModel;

namespace Enrich.IMS.Dto.Enums
{
    public enum LookupDataType
    {
        None = 0,

        #region SalesLead

        /// <summary>
        /// Table: Enum
        /// </summary>
        [Description("SaleLeadType")]
        SaleLeadType,

        /// <summary>
        /// Table: Enum
        /// </summary>
        [Description("SaleLeadStatus")]
        SaleLeadStatus,

        /// <summary>
        /// Table: SalesLeadInteractionStatus
        /// </summary>
        [Description("SalesLeadInteractionStatus")]
        SalesLeadInteractionStatus,

        #endregion

        #region Merchant

        /// <summary>
        /// Table: Enum
        /// </summary>
        [Description("Service Type")]
        ServiceType,

        /// <summary>
        /// Table: Enum
        /// </summary>
        [Description("Merchant Status")]
        MerchantStatus,

        /// <summary>
        /// Table: Enum
        /// </summary>
        [Description("Number of days created")]
        NODaysCreatedSearch,

        /// <summary>
        /// Table: Enum
        /// </summary>
        [Description("Remaining days")]
        RemainingDaySearch,

        /// <summary>
        /// Table: Enum
        /// </summary>
        [Description("Merchant Tab Name")]
        MerchantTabName,

        #endregion

        #region Member

        /// <summary>
        /// Table: Member
        /// </summary>
        [Description("AccountManager")]
        AccountManager,

        /// <summary>
        /// Table: Member
        /// </summary>
        [Description("SalesTeam")]
        SalesTeam,

        /// <summary>
        /// Table: Department, Member
        /// </summary>
        [Description("SalesTeam")]
        SalesMember,

        #endregion


        #region Ticket
        /// <summary>
        /// Table: TicketStatus
        /// </summary>
        [Description("TicketStatus")]
        TicketStatus,

        /// <summary>
        /// Table: TicketType
        /// </summary>
        [Description("TicketType")]
        TicketType,
        #endregion

        #region Partner

        /// <summary>
        /// Table: Partner
        /// </summary>
        [Description("Partner")]
        Partner,

        #endregion

        #region Supcription
        /// <summary>
        /// LicenseStatus
        /// </summary>
        [Description("LicenseStatus")]
        LicenseStatus

        #endregion

    }
}