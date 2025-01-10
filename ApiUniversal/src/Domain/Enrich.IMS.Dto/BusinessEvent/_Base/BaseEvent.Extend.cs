using Enrich.Dto.Base.Attributes;
using System;

namespace Enrich.IMS.Dto
{
    public partial class BaseEvent
    {
        /// <summary>
        /// Meta Data in business event
        /// </summary>
        public string MetaData { get; set; }
    }
}