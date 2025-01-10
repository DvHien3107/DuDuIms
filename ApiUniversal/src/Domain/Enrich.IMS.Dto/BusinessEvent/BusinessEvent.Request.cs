using Enrich.Dto.Base.Attributes;

namespace Enrich.IMS.Dto
{
    public partial class BusinessEventRequest
    {
        /// <summary>
        /// NameSpace
        /// </summary>
        public string SearchText { get; set; }

        /// <summary>
        /// NameSpace
        /// </summary>
        public string NameSpace { get; set; }

        /// <summary>
        /// Event
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// Event
        /// </summary>
        public string ExtendConditions { get; set; }

        /// <summary>
        /// ex: "xx DESC, yy ASC"
        /// </summary>
        public string SqlOrderby { get; set; }
    }
}