using Enrich.Dto.Base;
using Enrich.Services.Implement.Library;
using Enrich.Services.Interface.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Services.Implement
{
    public abstract class BaseService
    {
        #region Changes

        public virtual ChangeEntity<TEntity> GetChangeEntities<TDto, TEntity>(IEnumerable<TDto> sources, IBaseMapper mapper) where TDto : ItemStateDto
        {
            var changes = GetChanges(sources);

            return new ChangeEntity<TEntity>
            {
                Added = mapper.Map<IEnumerable<TEntity>>(changes.Added).ToList(),
                Modified = mapper.Map<IEnumerable<TEntity>>(changes.Modified).ToList(),
                Deleted = changes.Deleted.Select(a => a.Id).ToList()
            };
        }

        public virtual (List<TDto> Added, List<TDto> Modified, List<TDto> Deleted) GetChanges<TDto>(IEnumerable<TDto> sources) where TDto : ItemStateDto
        {
            return
            (
                Added: sources.Added().ToList(),
                Modified: sources.Modified().ToList(),
                Deleted: sources.Deleted().ToList()
            );
        }

        #endregion   
    }
}
