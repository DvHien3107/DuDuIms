using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Enum.IMS
{
    public enum OrderStatus
    {
        Open = 0,
        Closed = 1,
        Paid_Wait = 2,
        Canceled = -1
    }
}
