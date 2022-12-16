using Dapper.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;

namespace Dapper.Repository.SqlServer
{
    public class SqlServerContext<TUser> : DbContext<TUser>
    {
        private readonly string _connectionString;

        public SqlServerContext(string connectionString, ILogger logger, IEnumerable<IErrorMessageHandler> messageHandlers = null) : base(logger, messageHandlers)
        {
            _connectionString = connectionString;
        }

        public override char StartDelimiter => '[';

        public override char EndDelimiter => ']';

        public override string SelectIdentityCommand => "SELECT SCOPE_IDENTITY();";

        public override IDbConnection GetConnection() => new SqlConnection(_connectionString);        
    }
}
