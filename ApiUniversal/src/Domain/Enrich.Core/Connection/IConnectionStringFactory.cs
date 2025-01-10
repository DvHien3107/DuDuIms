using Enrich.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Core.Connection
{
    public interface IConnectionStringFactory
    {
        string GetConnectionString(string dbServer, string dbName);

        //string GetConnectionString(string customerName);

        string GetConnectionString(EnrichContext context);
    }
}
