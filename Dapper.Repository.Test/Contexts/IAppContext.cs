using BlazorAO.Models;
using Dapper.Repository.Test.Repositories;

namespace Dapper.Repository.Test.Contexts
{
    public enum ProfileSourceOptions
    {
        Anonymous,
        Database,
        Cache
    }

    public interface IAppContext
    {
        ProfileSourceOptions ProfileSource { get; }

        BaseRepository<Budget> Budgets { get; }
        BaseRepository<Client> Clients { get; }
        BaseRepository<Job> Jobs { get; }
        UserRepository Users { get; }
        WorkHoursRepository WorkHours { get; }
        BaseRepository<Workspace> Workspaces { get; }
    }
}