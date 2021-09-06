using AO.Models.Enums;
using BlazorAO.Models;
using Dapper.Repository.SqlServer;
using Dapper.Repository.Test.Queries;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository.Test.Repositories
{
    public class WorkHoursRepository : BaseRepository<WorkHours>
    {
        public WorkHoursRepository(SqlServerContext<UserInfoResult> context) : base(context)
        {
        }

        protected override async Task<(bool result, string message)> ValidateAsync(IDbConnection connection, SaveAction action, WorkHours model, IDbTransaction txn = null)
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
