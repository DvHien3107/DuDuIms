using Enrich.Dto.Base.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.EnumValue
{
    public class EnumValueDto
    {
        /// <summary>
        /// Id
        /// </summary>
        [FieldDb(nameof(Id))]
        public int Id { get; set; }

        /// <summary>
        /// Namespace
        /// </summary>
        [FieldDb(nameof(Namespace))] 
        public string Namespace { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        [FieldDb(nameof(Value))] 
        public string Value { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [FieldDb(nameof(Name))] 
        public string Name { get; set; }

        /// <summary>
        /// Text
        /// </summary>
        [FieldDb(nameof(Order))]
        public string Order { get; set; }


        /// <summary>
        /// IconUrl
        /// </summary>
        public string IconUrl { get; set; }

    }
}
