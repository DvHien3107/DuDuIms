using Enrich.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base.Requests
{
    public class DeleteRequest
    {
        public virtual bool IsValid => !Ids.IsNullOrEmpty();

        public int[] Ids { get; set; }

        /// <summary>
        /// <br>Global: 0 (delete), 1 (restore), 2 (force-delete)</br> 
        /// <br>Company: 1 (IsDeleted = 1 AND Persons.CompanyId = NULL)</br>
        /// <br>Company: 2 (IsDeleted = 1 AND Persons.IsDeleted = 1)</br>
        /// <br>Company: 3 (DELETE AND Persons.CompanyId = NULL)</br>
        /// <br>Company: 4 (IsDeleted = 0 -> restore)</br>
        /// <br>Company: 5 (IsDeleted = 0 -> restore AND Persons.IsDeleted = 0)</br>
        /// </summary>
        public int RequestType { get; set; }

        public int? RequestBy { get; set; }

        #region Task module

        /// <summary>
        /// Task module
        /// </summary>
        public int[] RecurrenceIds { get; set; }

        /// <summary>
        /// Task module -> capture values from FE
        /// </summary>
        public DeleteTaskItem[] TaskItems { get; set; }

        /// <summary>
        /// Task module -> capture values from FE
        /// </summary>
        public bool TaskIsApplySeries { get; set; }

        #endregion

        #region Internal

        /// <summary>
        /// make sure request-type is invalid
        /// </summary>
        internal bool? __SqlParamIsDeleted
        {
            get
            {
                switch (RequestType)
                {
                    case 1: // restore -> update IsDelete = false
                        return null;

                    case 2: // force-delete -> delete when IsDeleted = true              
                        return true;

                    case 0:
                    default:
                        return false;
                }
            }
        }

        #endregion

        public DeleteRequest(int[] ids) : this() => Ids = ids;

        public DeleteRequest() { }


        public class DeleteTaskItem
        {
            public int Id { get; set; }

            public int RecurrenceId { get; set; }

            public int ParentId { get; set; }
        }
    }
   
}
