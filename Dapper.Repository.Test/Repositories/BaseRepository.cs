using AO.Models.Enums;
using AO.Models.Interfaces;
using BlazorAO.Models.Conventions;
using Dapper.Repository.SqlServer;
using Dapper.Repository.Test.Queries;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository.Test.Repositories
{
    public class BaseRepository<TModel> : Repository<UserInfoResult, TModel, int> where TModel : IModel<int>
    {
        public BaseRepository(SqlServerContext<UserInfoResult> context) : base(context)
        {
        }

        protected override async Task BeforeSaveAsync(IDbConnection connection, SaveAction action, TModel model, IDbTransaction txn = null)
        {
            if (model is BaseTable baseTable)
            {
                switch (action)
                {
                    case SaveAction.Insert:
                        baseTable.CreatedBy = Context.User.Name;
                        baseTable.DateCreated = Context.User.LocalTime;
                        break;

                    case SaveAction.Update:
                        baseTable.ModifiedBy = Context.User.Name;
                        baseTable.DateModified = Context.User.LocalTime;
                        break;
                }
            }

            await Task.CompletedTask;
        }
    }
}
