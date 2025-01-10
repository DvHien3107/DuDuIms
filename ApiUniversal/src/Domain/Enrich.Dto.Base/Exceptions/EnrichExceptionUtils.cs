using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base.Exceptions
{
    public static class EnrichExceptionUtils
    {
        public static void Throw(EnrichException exception)
        {
            if (exception != null)
            {
                throw exception;
            }
        }

        public static void ThrowValidations(List<EnrichValidationFailure> failures, string genericMsg = "ValidationExceptions")
          => throw new EnrichValidationException(genericMsg) { ExtendData = failures };

        public static void ThrowMultiple(EnrichException[] exceptions, string genericMsg = "MultipleExceptions")
            => throw new EnrichMultipleException(genericMsg) { ExtendData = exceptions };

        public static EnrichException Throw(int code, string message, object extendData = null, int? httpStatusCode = null)
           => throw new EnrichException(code, message) { ExtendData = extendData, HttpStatusCode = httpStatusCode };

        public static EnrichException ThrowWithInner(int code, string message, Exception innerException, object extendData = null)
            => throw new EnrichException(code, message, innerException) { ExtendData = extendData };
        public static EnrichException ThrowWithoutNotify(int code, string message, object extendData = null, int? httpStatusCode = null, bool postBug = true, bool logError = true)
            => throw new EnrichException(code, message, postBug: postBug, logError: logError) { ExtendData = extendData, HttpStatusCode = httpStatusCode };

    }
}
