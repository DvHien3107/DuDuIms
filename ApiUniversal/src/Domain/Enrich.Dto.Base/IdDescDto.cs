using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base
{
    public class IdDescDto 
    {
        public int Id { get; set; }

        #region Constructors

        public IdDescDto(int id) : this() => Id = id;

        public IdDescDto() { }

        #endregion
    }
}
