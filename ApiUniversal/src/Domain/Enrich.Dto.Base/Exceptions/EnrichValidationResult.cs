using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base.Exceptions
{
    public class EnrichValidationResult
    {
        public List<EnrichValidationFailure> Errors { get; set; } = new List<EnrichValidationFailure>();

        public bool IsValid => Errors.Count == 0;

        public EnrichValidationResult()
        {
        }

        public EnrichValidationResult(IEnumerable<EnrichValidationFailure> errors)
            : this()
        {
            Errors = errors.Where(a => a != null).ToList();
        }
    }
}
