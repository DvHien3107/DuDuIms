using Inner.Libs.Helpful;

namespace EnrichcousBackOffice.Utils.IEnums
{

    /// <summary>
    /// IMS Version
    /// </summary>
    public enum InvoiceStatus
    {
        [EnumAttr("Open", "Unpaid")] Open = 0,
        [EnumAttr("Payment Later", "Payment later")] PaymentLater = 1,
        [EnumAttr("Paid/Wait", "Paid & not delivered - Invoice has hardware items")] Paid_Wait = 2,
        [EnumAttr("Closed", "Paid & delivered")] Closed = 3,
        [EnumAttr("Canceled", "Invoice canceled")] Canceled = 4,
    }
}