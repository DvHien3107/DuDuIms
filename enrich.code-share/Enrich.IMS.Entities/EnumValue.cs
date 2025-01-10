using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Entities
{
    /// <summary>
    /// System value, user can not add or update
    /// </summary>
    public class EnumValue
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Namespace
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// IconUrl
        /// </summary>
        public string IconUrl { get; set; }

        /// <summary>
        /// Text
        /// </summary>
        public int Order { get; set; }

    }
}
