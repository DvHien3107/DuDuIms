using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto
{
    public sealed partial class SqlColumns
    {
        public sealed class BonusStrategySetting
        {
            public const string ApplyForMemberTypeName = "ApplyForMemberType_Name";
            public const string OptNewMemberTotalEqualOrThan = "Opt_NewMemberTotal_EqualOrThan";
            public const string OptTotalIncomeEqualOrThan = "Opt_TotalIncome_EqualOrThan";
            public const string Opt_TotalQuantityFullContracts_EqualOrThan = "Opt_TotalQuantityFullContracts_EqualOrThan";
        }
    }
}
