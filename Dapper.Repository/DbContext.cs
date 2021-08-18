using Microsoft.Extensions.Logging;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository
{
    public abstract class DbContext<TUser>
    {        
        public DbContext(ILogger logger)
        {            
            Logger = logger;
        }        
        
        public ILogger Logger { get; }        
        public TUser User { get; internal set; }

        public abstract IDbConnection GetConnection();
        public abstract char StartDelimiter { get; }
        public abstract char EndDelimiter { get; }
        public abstract string SelectIdentityCommand { get; }

        public async Task<TUser> GetUserAsync()
        {
            if (User is null)
            {
                using var cn = GetConnection();
                User = await QueryUserAsync(cn);
            }

            return User;
        }

        /// <summary>
        /// override this to get info about the current user.
        /// Use a caching solution in your application to avoid unnecessary database round trips
        /// </summary>
        protected virtual async Task<TUser> QueryUserAsync(IDbConnection connection) => await Task.FromResult(default(TUser));
    }
}
