
namespace Enrich.IMS.Dto
{
    public partial class BaseEvent<TValue> : BaseEvent
    {
        public TValue Value { get; set; }
    }
}