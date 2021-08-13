using AO.Models.Interfaces;
using System.Data;

namespace Dapper.Repository
{
    public abstract class DbContext
    {        
        public DbContext(IUserBase user)
        {
            User = user;
        }

        public IUserBase User { get; }
        public abstract IDbConnection GetConnection();
        public abstract char StartDelimiter { get; }
        public abstract char EndDelimiter { get; }
        public abstract string SelectIdentityCommand { get; }
    }
}
