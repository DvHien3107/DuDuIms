using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Requests
{
    public class BaseSaveByJsonPatchRequest<TDto> where TDto : class
    {
        public bool IsNew { get; set; }

        public bool ValidateData { get; set; } = true;

        public TDto NewDto { get; set; }

        public TDto OldDto { get; set; }

        public List<PatchOperationDto> PatchChanges { get; set; }

        public bool HasAnyChange(params string[] fields)
        {
            foreach (var field in fields)
            {
                var hasChange = HasChange(field);
                if (hasChange)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasChange(string field, string subField = "")
        {
            if (PatchChanges == null || PatchChanges.Count == 0 || PatchChanges.All(a => !a.Field.Equals(field)))
            {
                return false;
            }

            return string.IsNullOrWhiteSpace(subField) || PatchChanges.Any(a => a.Field.Equals(field) && a.FullField.EndsWith(subField));
        }
    }

    public class BaseSaveByJsonPatchResponse<TDto> where TDto : class
    {
        public int ObjectId { get; set; }

        public TDto Object { get; set; }
    }
}
