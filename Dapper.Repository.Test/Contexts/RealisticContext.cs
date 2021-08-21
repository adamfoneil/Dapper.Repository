using AO.Models.Interfaces;
using BlazorAO.Models;
using Dapper.Repository.SqlServer;
using Dapper.Repository.Test.Extensions;
using Dapper.Repository.Test.Queries;
using Dapper.Repository.Test.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SqlServer.LocalDb;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository.Test.Contexts
{
    public class RealisticContext : SqlServerContext<IUserBase>, IAppContext
    {
        private readonly IDistributedCache _cache;
        // even more realistic use would be AuthenticationStateProvider, but I'm not sure how to make that testable
        private readonly string _userName; 

        public RealisticContext(string userName, IDistributedCache cache, ILogger logger) : base(LocalDb.GetConnectionString(SimpleContext.DbName), logger)
        {
            _cache = cache;
            _userName = userName;
        }

        public ProfileSourceOptions ProfileSource { get; private set; }

        protected override async Task<IUserBase> QueryUserAsync(IDbConnection connection)
        {            
            if (string.IsNullOrEmpty(_userName)) return null;

            var key = $"userInfo.{_userName}";
            var result = await _cache.GetItemAsync<UserInfoResult>(key);
            ProfileSource = ProfileSourceOptions.Cache;

            if (result == default(UserInfoResult))
            {
                result = await new UserInfo() { UserName = _userName }.ExecuteSingleOrDefaultAsync(connection);                
                result.Permissions = await new UserPermissions() { UserName = _userName }.ExecuteAsync(connection);
                await _cache.SetItemAsync(key, result);
                ProfileSource = ProfileSourceOptions.Database;
            }

            return result;
        }

        public BaseRepository<Workspace> Workspaces => new BaseRepository<Workspace>(this);
        public BaseRepository<Client> Clients => new BaseRepository<Client>(this);
        public BaseRepository<Job> Jobs => new BaseRepository<Job>(this);
        public BaseRepository<Budget> Budgets => new BaseRepository<Budget>(this);

        // because dbo.AspNetUsers insists on doing its own thing
        public UserRepository Users => new UserRepository(this);

        // because it has unique validation
        public WorkHoursRepository WorkHours => new WorkHoursRepository(this);
    }
}
