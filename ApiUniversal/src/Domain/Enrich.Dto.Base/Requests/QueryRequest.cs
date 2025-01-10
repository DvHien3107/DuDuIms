using Enrich.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Requests
{
    public abstract class QueryRequest
    {
        public int[] InIds { get; set; }

        public int[] NotInIds { get; set; }

        public string StartsWith { get; set; }

        public Language? StartsWithLanguage { get; set; }

        public string StartsWithPrefixFieldName { get; set; } = "Desc";

        #region Custom

        public static TRequest FromBaseRequest<TRequest>(QueryRequest baseRequest) where TRequest : QueryRequest, new()
        {
            return new TRequest
            {
                InIds = baseRequest.InIds,
                NotInIds = baseRequest.NotInIds,
                StartsWith = baseRequest.StartsWith,               
                StartsWithPrefixFieldName = baseRequest.StartsWithPrefixFieldName
            };
        }

        public static TRequest FromValues<TRequest>(string searchText = null,int[] inIds = null, int[] notInIds = null) where TRequest : QueryRequest, new()
        {
            return new TRequest
            {
                InIds = inIds,
                NotInIds = notInIds,
                StartsWith = searchText              
            };
        }

        #endregion

    }
}
