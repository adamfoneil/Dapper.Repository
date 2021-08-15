using BlazorAO.Models;
using Dapper.Repository.SqlServer;
using Dapper.Repository.Test.Repositories;
using Microsoft.Extensions.Logging;
using SqlServer.LocalDb;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository.Test
{
    public class MyContext : SqlServerContext<User>
    {
        public const string DbName = "DapperRepository";

        public MyContext(string userName, ILogger logger) : base(LocalDb.GetConnectionString(DbName), userName, logger)
        {
        }

        public override async Task<User> GetUserAsync(IDbConnection connection, string userName) => await Task.FromResult(new User()
        {
            Name = userName
        });

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
