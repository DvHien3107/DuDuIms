//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Enrich.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class C_Questionare_Technician
    {
        public string Id { get; set; }
        public string QuestionareId { get; set; }
        public string Name { get; set; }
        public string NickName { get; set; }
        public string Commission { get; set; }
        public string PayrollSplitCheckOrCash { get; set; }
        public Nullable<bool> AddDiscounts { get; set; }
        public Nullable<bool> AdjustPrices { get; set; }
        public Nullable<System.DateTime> CreateAt { get; set; }
    }
}
