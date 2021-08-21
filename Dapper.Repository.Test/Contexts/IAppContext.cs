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

    /// <summary>
    /// enables use of Simple and Realistic contexts with the same integration tests
    /// </summary>
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