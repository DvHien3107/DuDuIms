using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base.Attributes
{
    public class LoadOrUpdateOptionAttribute : Attribute
    {
        public string PropName { get; }

        public LoadOrUpdateOptionAttribute(string propName) => PropName = propName;
    }
}
