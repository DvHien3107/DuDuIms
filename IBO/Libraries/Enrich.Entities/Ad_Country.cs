using Enrich.Core.UnitOfWork.Data;
using System;
using System.Collections.Generic;

namespace Enrich.Entities
{    
    public partial class Ad_Country : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneCode { get; set; }
        public string CountryCode { get; set; }
    }
}
