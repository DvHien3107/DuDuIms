using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Enrich.Common.Enums
{
    public sealed class SendGridTemplate
    {
        public const string PaidSuccessfully = "d-d9aa19363d68438ab0d5ead68d909d3b";
        public const string PaymentFailed = "d-c84553eb18224c28a72f1d4bea27a358";
        public const string FreeStyle = "d-48ab6a08f8644885b087a27d55a183f2";
    }
}