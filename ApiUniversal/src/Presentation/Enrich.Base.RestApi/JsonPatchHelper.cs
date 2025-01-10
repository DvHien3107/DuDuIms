using Enrich.Dto;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Base.RestApi
{
    public static class JsonPatchHelper
    {
        public static void ApplyTo<T>(T fullObject, List<Operation<T>> operations) where T : class
        {
            var patch = new JsonPatchDocument<T>(operations, new DefaultContractResolver());
            patch.ApplyTo(fullObject);
        }

        public static List<PatchOperationDto> GetOperationsAsDto<T>(List<Operation<T>> operations) where T : class
        {
            return operations
                .Where(a => !string.IsNullOrWhiteSpace(a.path))
                .Select(x => new PatchOperationDto
                {
                    FullField = x.path,
                    Field = x.path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim(),
                    Value = x.value,
                }).ToList();
        }

        public static List<string> GetChangedPropNames<T>(List<Operation<T>> operations) where T : class
        {
            return operations
                .Select(a => a.path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .ToList();
        }
    }
}
