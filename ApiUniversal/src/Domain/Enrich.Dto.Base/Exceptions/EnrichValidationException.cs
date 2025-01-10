using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base.Exceptions
{
    public class EnrichValidationException : EnrichException
    {
        public EnrichValidationException(string message, bool postBug = false, bool logError = false)
            : base(ExceptionCodes.ValidationExceptions, message, postBug, logError)
        {
        }

        public EnrichValidationException(string message, Exception innerException, bool postBug = false, bool logError = false)
            : base(ExceptionCodes.ValidationExceptions, message, innerException, postBug, logError)
        {
        }
    }
}
