using Enrich.Dto.Base.Attributes;
using System;

namespace Enrich.IMS.Dto
{
    [FieldDb(table: $"{SqlTables.OrderEvent}")]
    public partial class OrderEventDto : BaseEvent
    {
    }
}