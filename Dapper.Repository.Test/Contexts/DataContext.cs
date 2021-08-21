using AO.Models.Interfaces;
using BlazorAO.Models;
using Dapper.Repository.SqlServer;
using Dapper.Repository.Test.Extensions;
using Dapper.Repository.Test.Queries;
using Dapper.Repository.Test.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SqlServer.LocalDb;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository.Test.Contexts
{
    public enum ProfileSourceOptions
    {
        Anonymous,
        Database,
        Cache
    }

    public class DataContext : SqlServerContext<UserInfoResult>
    {
        private readonly IDistributedCache _cache;
        // even more realistic use would be AuthenticationStateProvider, but I'm not sure how to make that testable
        private readonly string _userName;

        public const string DbName = "DapperRepository";

        public DataContext(string userName, IDistributedCache cache, ILogger logger) : base(LocalDb.GetConnectionString(DbName), logger)
        {
            _cache = cache;
            _userName = userName;
        }

        public ProfileSourceOptions ProfileSource { get; private set; }

        protected override async Task<UserInfoResult> QueryUserAsync(IDbConnection connection)
        {
            ProfileSource = ProfileSourceOptions.Anonymous;
            if (string.IsNullOrEmpty(_userName)) return null;
            
            var result = await _cache.GetItemAsync<UserInfoResult>(CacheKey);
            ProfileSource = ProfileSourceOptions.Cache;

            if (result == default(UserInfoResult))
            {
                result = await new UserInfo() { UserName = _userName }.ExecuteSingleOrDefaultAsync(connection);                
                result.Permissions = await new UserPermissions() { UserName = _userName }.ExecuteAsync(connection);
                await _cache.SetItemAsync(CacheKey, result);
                ProfileSource = ProfileSourceOptions.Database;
            }

            return result;
        }

        protected override async Task OnLogoutAsync() => await _cache.RemoveAsync(CacheKey);        

        private string CacheKey => $"userInfo.{_userName}";

        /// <summary>
        /// this is just for test methods to force cache use
        /// </summary>        
        public async Task CacheUserAsync()
        {
            var user = await GetUserAsync();
            await _cache.SetItemAsync(CacheKey, user as UserInfoResult);            
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
