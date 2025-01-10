using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base.Exceptions
{
    public sealed partial class ExceptionCodes
    {
        public const int MultipleExceptions = 2000000000;
        public const int ValidationExceptions = 2000000001;

        //10000-19999
        public const int Generic_Error = 10000;

        public const int Generic_CannotCreate = 10001;

        public const int Generic_CannotSave = 10002;

        public const int Generic_CannotDelete = 10003;

        public const int Generic_UpdateDataInvalid = 10004;

        public const int Generic_NotFound = 10005;

        //Ticket: 20000 - 20999

        //Invoice: 21000 - 21999

        //Sale lead: 22000 - 22999

        //System: 23000 - 23999
    }
}
