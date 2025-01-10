using Enrich.Dto.Base.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enrich.IMS.Entities
{    
    public partial class OptionConfigDto
    {
        [FieldDb(nameof(Id))]
        public int Id { get; set; }

        [FieldDb(nameof(Key))]
        public string Key { get; set; }

        [FieldDb(nameof(Value))]
        public string Value { get; set; }

        [FieldDb(nameof(Description))]
        public string Description { get; set; }
    }
}
