using Enrich.Common.Enums;
using Enrich.Dto.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Services.Implement.Library
{
    public static class ExtendMethods
    {
        #region Changes

        public static IEnumerable<T> Added<T>(this IEnumerable<T> sources) where T : IItemState
            => sources.Where(a => a.ItemState == ItemStateType.Added);

        public static IEnumerable<T> Modified<T>(this IEnumerable<T> sources) where T : IItemState
            => sources.Where(a => a.ItemState == ItemStateType.Modified);

        public static IEnumerable<T> Deleted<T>(this IEnumerable<T> sources) where T : IItemState
           => sources.Where(a => a.ItemState == ItemStateType.Deleted);

        #endregion
    }
}
