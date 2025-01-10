using System;

namespace Enrich.Dto.Base.Exceptions
{
    public class EnrichException : Exception
    {
        public int InternalCode { get; }

        public bool PostBug { get; set; } = true;

        public bool LogError { get; set; } = true;

        public object ExtendData { get; set; }

        public int? HttpStatusCode { get; set; }

        public EnrichException(int internalCode, string message, bool postBug = true, bool logError = true)
            : base(message)
        {
            InternalCode = internalCode;
            PostBug = postBug;
            LogError = logError;
        }

        public EnrichException(int internalCode, string message, Exception innerException, bool postBug = true, bool logError = true)
            : base(message, innerException)
        {
            InternalCode = internalCode;
            PostBug = postBug;
            LogError = logError;
        }
    }
}
