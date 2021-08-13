using AO.Models.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Dapper.Repository.SqlServer
{
    public class SqlServerContext : DbContext
    {
        private readonly string _connectionString;

        public SqlServerContext(string connectionString, IUserBase user) : base(user)
        {
            _connectionString = connectionString;
        }

        public override char StartDelimiter => '[';

        public override char EndDelimiter => ']';

        public override string SelectIdentityCommand => "SELECT SCOPE_IDENTITY();";

        public override IDbConnection GetConnection() => new SqlConnection(_connectionString);        
    }
}
