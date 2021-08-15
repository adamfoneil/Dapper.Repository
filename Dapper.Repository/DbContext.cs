using Microsoft.Extensions.Logging;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository
{
    public abstract class DbContext<TUser>
    {        
        public DbContext(string userName, ILogger logger)
        {
            UserName = userName;
            Logger = logger;
        }        
        
        public ILogger Logger { get; }
        public string UserName { get; }
        public TUser User { get; set; }

        public abstract IDbConnection GetConnection();
        public abstract char StartDelimiter { get; }
        public abstract char EndDelimiter { get; }
        public abstract string SelectIdentityCommand { get; }

        /// <summary>
        /// override this to get info about the current user.
        /// Use a caching solution in your application to avoid unnecessary database round trips
        /// </summary>
        public virtual async Task<TUser> GetUserAsync(IDbConnection connection, string userName) => await Task.FromResult(default(TUser));
    }
}
