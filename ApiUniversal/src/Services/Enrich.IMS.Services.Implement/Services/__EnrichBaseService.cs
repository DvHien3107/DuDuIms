using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Core.Infrastructure.Repository;
using Enrich.Dto.Base;
using Enrich.Services.Implement;
using Enrich.Services.Interface.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public abstract class EnrichBaseService<T, TDto> : GenericService<T, TDto> where TDto : class
    {
        protected EnrichBaseService(IGenericRepository<T> genericRepository)
            : base(genericRepository)
        {
        }

        protected EnrichBaseService(IGenericRepository<T> genericRepository, IBaseMapper mapper)
            : base(genericRepository, mapper)
        {
        }

        public string NewLine => Environment.NewLine;   

        #region ItemState

        protected void OptimizeItemStates<TItem>(bool isNew, List<TItem> sources, Action<TItem> processItem = null) where TItem : IItemState
        {
            if (isNew)
                sources.RemoveAll(a => a.ItemState == ItemStateType.Deleted);

            foreach (var item in sources)
            {
                processItem?.Invoke(item);

                if (isNew)
                    item.ItemState = ItemStateType.Added;

            }
        }

        protected bool HasChangeItemStates<TItem>(List<TItem> sources) where TItem : IItemState
        {
            if (sources == null || sources.Count == 0)
                return false;

            return sources.Any(a => a.ItemState != ItemStateType.Unchanged);
        }

        #endregion

        #region LoadTab

        protected List<TLoadTabType> GetLoadTabTypes<TLoadTabType>(string tabs, TLoadTabType? defTabType = null) where TLoadTabType : struct
        {
            var loadTabs = new List<TLoadTabType>();
            if (tabs == "*")
            {
                loadTabs = EnumHelper.ToList<TLoadTabType>();
            }
            else if (!string.IsNullOrWhiteSpace(tabs))
            {
                loadTabs.AddRange(tabs.SplitEx(distinct: true).Select(a => a.GetEnumNull<TLoadTabType>()).Where(tab => tab != null).Select(tab => tab.Value));
            }

            if (loadTabs.Count == 0 && defTabType != null)
            {
                loadTabs.Add(defTabType.Value);
            }

            return loadTabs;
        }

        #endregion
    }
}
