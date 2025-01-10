using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Task
{
    public class TaskTemplateFieldModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TypeField { get; set; }

        public int? DisplayOrder { get; set; }

    }
}