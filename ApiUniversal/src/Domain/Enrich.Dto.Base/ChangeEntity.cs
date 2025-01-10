using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base
{
    public class ChangeEntity<TEntity> : ChangeEntity
    {
        #region Added

        public List<TEntity> Added { get; set; }

        public List<string> IgnoreInsertFields { get; set; } = new List<string>();

        #endregion

        #region Modified

        public List<TEntity> Modified { get; set; }

        public List<string> IgnoreUpdateFields { get; set; } = new List<string>();

        #endregion

        #region Deleted

        public List<int> Deleted { get; set; }

        public bool UpdateIsDeleted { get; set; }

        #endregion

        #region Custom

        /// <summary>
        /// Any in added, modified, deleted
        /// </summary>
        public bool HasChanges => HasAdded || HasModified || HasDeleted;

        public bool HasAdded => Added?.Count > 0;

        public bool HasModified => Modified?.Count > 0;

        public bool HasDeleted => Deleted?.Count > 0;

        #endregion

        public ChangeEntity(IEnumerable<TEntity> added = null, IEnumerable<TEntity> modified = null, IEnumerable<int> deleted = null)
        {
            if (added != null) Added = new List<TEntity>(added);
            if (modified != null) Modified = new List<TEntity>(modified);
            if (deleted != null) Deleted = new List<int>(deleted);
        }
    }

    public class ChangeEntity
    {
        public static ChangeEntity<IItem> FromAdded<IItem>(IEnumerable<IItem> added) => new ChangeEntity<IItem>(added: added);

        public static ChangeEntity<IItem> FromModified<IItem>(IEnumerable<IItem> modified) => new ChangeEntity<IItem>(modified: modified);
    }
}
