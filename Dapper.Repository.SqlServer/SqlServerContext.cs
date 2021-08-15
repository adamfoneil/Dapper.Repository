﻿using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Dapper.Repository.SqlServer
{
    public class SqlServerContext : DbContext
    {
        private readonly string _connectionString;

        public SqlServerContext(string connectionString, string userName, ILogger logger) : base(userName, logger)
        {
            _connectionString = connectionString;
        }

        public override char StartDelimiter => '[';

        public override char EndDelimiter => ']';

        public override string SelectIdentityCommand => "SELECT SCOPE_IDENTITY();";

        public override IDbConnection GetConnection() => new SqlConnection(_connectionString);        
    }
}
