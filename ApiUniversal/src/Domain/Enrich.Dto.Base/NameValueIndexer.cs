using Enrich.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base
{
    public class NameValueIndexer<TValue> //System.Dynamic.DynamicObject
    {
        private readonly Func<string, TValue> _getValue;

        public NameValueIndexer(Func<string, TValue> getValue)
        {
            _getValue = getValue;
        }

        public TValue this[string name] => _getValue(name);

        public TEnum AsEnum<TEnum>(string name, TEnum defEnum = default(TEnum)) where TEnum : struct
        {
            object queryValue = this[name];
            return queryValue != null ? queryValue.GetEnum(defEnum) : defEnum;
        }
    }
}
