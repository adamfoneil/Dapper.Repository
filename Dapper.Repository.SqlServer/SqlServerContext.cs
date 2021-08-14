using AO.Models.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Dapper.Repository.SqlServer
{
    public class SqlServerContext<TUser> : DbContext<TUser> where TUser : IUserBase
    {
        private readonly string _connectionString;

        public SqlServerContext(string connectionString, TUser user, ILogger logger) : base(user, logger)
        {
            _connectionString = connectionString;
        }

        public override char StartDelimiter => '[';

        public override char EndDelimiter => ']';

        public override string SelectIdentityCommand => "SELECT SCOPE_IDENTITY();";

        public override IDbConnection GetConnection() => new SqlConnection(_connectionString);        
    }
}
