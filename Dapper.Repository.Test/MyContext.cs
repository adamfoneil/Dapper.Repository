using BlazorAO.Models;
using Dapper.Repository.SqlServer;
using Dapper.Repository.Test.Repositories;
using Microsoft.Extensions.Logging;
using SqlServer.LocalDb;

namespace Dapper.Repository.Test
{
    public class MyContext : SqlServerContext
    {
        public const string DbName = "DapperRepository";

        public MyContext(ILogger logger) : base(LocalDb.GetConnectionString(DbName), new User() { Name = "adamo" }, logger)
        {
        }

        public BaseRepository<Workspace> Workspaces => new BaseRepository<Workspace>(this);
        public BaseRepository<Client> Clients => new BaseRepository<Client>(this);
        public BaseRepository<Job> Jobs => new BaseRepository<Job>(this);
        public BaseRepository<Budget> Budgets => new BaseRepository<Budget>(this);
        public WorkHoursRepository WorkHours => new WorkHoursRepository(this); // gets its own repo because it has unique validation
    }

}
