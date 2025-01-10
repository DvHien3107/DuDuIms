using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.ViewControler
{
    public class QuestTechViewService
    {
        public int Key { get; set; }
        public string Id { get; set; }
        public string QuestionareId { get; set; }
        public string Name { get; set; }
        public string NickName { get; set; }
        public string Commission { get; set; }
        public string PayrollSplitCheckOrCash { get; set; }
        public Nullable<bool> AddDiscounts { get; set; }
        public Nullable<bool> AdjustPrices { get; set; }
    }
}