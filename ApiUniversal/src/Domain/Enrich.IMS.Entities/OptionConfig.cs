using System.ComponentModel.DataAnnotations.Schema;

namespace Enrich.IMS.Entities
{    
    public partial class OptionConfig
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
}
