using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Common.Enums
{
    public enum FixedColAlign
    {
        None = 0,

        Left,

        Right
    }

    public sealed class ColDataType
    {
        public const string Text = "text";

        public const string Number = "number";

        public const string DateTime = "datetime";

        public const string Boolean = "boolean";
    }

    public sealed class ColDisplayType
    {
        public const string Text = "text";

        public const string Number = "number";

        public const string DateTime = "datetime";

        public const string Boolean = "boolean";

        public const string Currency = "currency";

        public const string Date = "date";

        public const string Time = "time";

        public const string Color = "color";

        public const string Picture = "picture";

        public const string Phone = "phone";

        public const string Email = "email";

        public const string Icon = "icon";
    }
}
