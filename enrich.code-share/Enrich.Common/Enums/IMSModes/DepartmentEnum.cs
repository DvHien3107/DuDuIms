using System.ComponentModel.DataAnnotations;

namespace Enrich.Common.Enums
{
    public class DepartmentEnum
    {
        public enum Type
        {
            DEVELOPMENT,
            DEPLOYMENT,
            SUPPORT,
            FINANCE,
            SALES
        }

        public enum Status
        {
            Unactive = 0,
            Active = 1
        }
    }
}