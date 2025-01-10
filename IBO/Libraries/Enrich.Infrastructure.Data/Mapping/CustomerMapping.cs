using Enrich.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.Data.Mapping
{
    public class CustomerMapping : EntityTypeConfiguration<C_Customer>
    {

        public CustomerMapping()
        {
            // Primary Key
            this.HasKey(t => t.Id).Property(i => i.Id);
            
            // Table & Column Mappings
            this.ToTable("C_Customer");

        }    
    }
}
