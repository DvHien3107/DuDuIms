using Enrich.Core.Connection;
using Enrich.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.Data.Dapper
{
    public class EnrichIMSConnectionFactory : BaseConnectionFactory, IConnectionFactory
    {
        private readonly EnrichContext _context;
        private readonly IConnectionStringFactory _connectionStringFactory;

        public EnrichIMSConnectionFactory(string connectionString)
            : base(connectionString)
        {
        }

        public EnrichIMSConnectionFactory(EnrichContext enrichContextContext, IConnectionStringFactory enrichContext2007ConnectionStringFactory)
            : this(string.Empty)
        {
            _context = enrichContextContext;
            _connectionStringFactory = enrichContext2007ConnectionStringFactory;
        }

        public override IDbConnection GetDbConnection()
        {
            InitConnectionStringFromEnrichContextContext();
            return base.GetDbConnection();
        }

        public override string ConnectionString
        {
            get
            {
                InitConnectionStringFromEnrichContextContext();
                return base.ConnectionString;
            }
        }

        private void InitConnectionStringFromEnrichContextContext()
        {
            if (string.IsNullOrWhiteSpace(_connectionString) && _context != null && _connectionStringFactory != null)
            {
                _connectionString = _connectionStringFactory.GetConnectionString(_context);
            }
        }
    }
}
