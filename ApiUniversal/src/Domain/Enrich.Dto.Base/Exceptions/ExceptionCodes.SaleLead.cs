
namespace Enrich.Dto.Base.Exceptions
{
    public partial class ExceptionCodes
    {
        //26000 - 26999 Customer / merchant
        public const int Customer_NotFound = 26000;
        public const int Customer_SaveError = 26001;
        public const int Customer_SomeIdsCannotDelete = 26002;
        public const int Customer_MissingIdsToDelete = 26003;

        //27000 - 27999 SalesLead
        public const int SaleLead_NotFound = 27000;
        public const int SaleLead_SaveError = 27001;
        public const int SaleLead_SomeIdsCannotDelete = 27002;
        public const int SaleLead_MissingIdsToDelete = 27003;

        //28000 - 289999 SalesLead Comment
        public const int SaleLeadComment_NotFound = 28000;
        public const int SaleLeadComment_SaveError = 28001;

        //29000 - 29999 SalesLead Comment
        public const int Ticket_NotFound = 29000;
        public const int Ticket_SaveError = 29001;
    }
}
