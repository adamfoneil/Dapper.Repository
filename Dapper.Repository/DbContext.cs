using Microsoft.Extensions.Logging;
using System.Data;

namespace Dapper.Repository
{
    public abstract class DbContext
    {        
        public DbContext(ILogger logger)
        {            
            Logger = logger;
        }        
        
        public ILogger Logger { get; }
        public abstract IDbConnection GetConnection();
        public abstract char StartDelimiter { get; }
        public abstract char EndDelimiter { get; }
        public abstract string SelectIdentityCommand { get; }
    }
}
