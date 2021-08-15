using Microsoft.Extensions.Logging;
using System.Data;

namespace Dapper.Repository
{
    public abstract class DbContext
    {        
        public DbContext(string userName, ILogger logger)
        {
            UserName = userName;
            Logger = logger;
        }        
        
        public ILogger Logger { get; }
        public string UserName { get; }
        public abstract IDbConnection GetConnection();
        public abstract char StartDelimiter { get; }
        public abstract char EndDelimiter { get; }
        public abstract string SelectIdentityCommand { get; }
    }
}
