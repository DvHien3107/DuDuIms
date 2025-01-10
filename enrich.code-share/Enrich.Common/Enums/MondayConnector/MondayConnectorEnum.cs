using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Enrich.Common.Enums
{
    public class MondayConnectorEnum
    {
        public enum Type
        {
            SalesLead = 100,

            Member = 200,

            MemberEmail = 210,
        }

        public enum QueueType
        {
            syncing,

            create_pulse,

            update_name,

            update_column_value,
        }
    }
}