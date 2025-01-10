using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base.Exceptions
{
    public class EnrichMultipleException : EnrichException
    {
        public EnrichMultipleException(string message, bool postBug = true, bool logError = true)
            : base(ExceptionCodes.MultipleExceptions, message, postBug, logError)
        {
        }

        public EnrichMultipleException(string message, Exception innerException, bool postBug = true, bool logError = true)
            : base(ExceptionCodes.MultipleExceptions, message, innerException, postBug, logError)
        {
        }
    }
}
