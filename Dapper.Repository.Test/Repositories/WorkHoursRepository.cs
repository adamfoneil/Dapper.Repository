using BlazorAO.Models;
using Dapper.Repository.SqlServer;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository.Test.Repositories
{
    public class WorkHoursRepository : BaseRepository<WorkHours>
    {
        public WorkHoursRepository(SqlServerContext<User> context) : base(context)
        {
        }

        protected override async Task<(bool result, string message)> ValidateAsync(IDbConnection connection, WorkHours model, IDbTransaction txn = null)
        {
            await Task.CompletedTask;

            if (model.Hours <= 0)
            {
                return (false, "Hours must be greater than zero.");
            }

            return (true, null);
        }
    }
}
