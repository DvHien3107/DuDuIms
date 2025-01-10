using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Respons
{
    public class EmailServiceException : Exception
    {
        public string Body { get; set; }

        public EmailServiceException(string message, string body) : base(message)
        {
            Body = body;
        }

        public EmailServiceException(string message, string body, Exception innerException) : base(message, innerException)
        {
            Body = body;
        }
    }
    public class EmailResponse
    {
        public DateTime? DateSent { get; set; }
        public string? UniqueMessageId { get; set; }
    }
}
