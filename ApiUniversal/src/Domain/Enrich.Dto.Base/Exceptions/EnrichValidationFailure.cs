using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base.Exceptions
{
    public class EnrichValidationFailure
    {
        public string FullField { get; set; }

        public string ModelField { get; set; }

        public string Rules { get; set; }

        public string Scope { get; set; }

        public string Message { get; set; }

        public object ExternalFailure { get; set; }

        public override string ToString() => !string.IsNullOrWhiteSpace(Message) ? $"{FullField}: {Message}" : base.ToString();
    }
}
