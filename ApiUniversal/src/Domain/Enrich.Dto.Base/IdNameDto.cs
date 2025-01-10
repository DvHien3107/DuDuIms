using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base
{
    public class IdNameDto 
    {
        internal string Id { get; set; }

        public string Value { get; set; }

        public string Name { get; set; }

        public string IconUrl { get; set; }

        #region Constructors

        public IdNameDto(string id) : this() => Id = id;

        public IdNameDto() { }

        #endregion
    }
}
