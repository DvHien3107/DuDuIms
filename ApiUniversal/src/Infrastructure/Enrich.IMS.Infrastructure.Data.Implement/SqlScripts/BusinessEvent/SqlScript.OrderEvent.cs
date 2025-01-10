using Enrich.Common.Enums;
using Enrich.IMS.Dto;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static class OrderEvent
        {
            private const string Alias = SqlTables.OrderEvent;

            /// <summary>
            /// Get list event waiting precess by event name
            /// </summary>
            /// <param name="eventName"></param>
            /// <returns></returns>
            public static string GetListWaiting(string eventName) => $"SELECT * FROM {Alias} WITH (NOLOCK) WHERE Status = 0 AND [Event] = '{eventName}'";
        }
    }
}
