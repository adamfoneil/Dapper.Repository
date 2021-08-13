using AO.Models.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Dapper.Repository
{
    public abstract class DbContext
    {        
        public DbContext(IUserBase user, ILogger logger)
        {
            User = user;
            Logger = logger;
        }

        public IUserBase User { get; }
        public ILogger Logger { get; }
        public abstract IDbConnection GetConnection();
        public abstract char StartDelimiter { get; }
        public abstract char EndDelimiter { get; }
        public abstract string SelectIdentityCommand { get; }
    }
}
