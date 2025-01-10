using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.ViewControler
{
    public class QuestNonTechViewService
    {
        public int Key { get; set; }
        public string Id { get; set; }
        public string QuestionareId { get; set; }
        public string Name { get; set; }
        public string Job { get; set; }
        public string Pay { get; set; }
        public Nullable<bool> AdjustViewPayroll { get; set; }
    }
}