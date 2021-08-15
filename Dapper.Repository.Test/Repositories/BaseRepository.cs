using AO.Models.Enums;
using AO.Models.Interfaces;
using BlazorAO.Models.Conventions;
using Dapper.Repository.SqlServer;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository.Test.Repositories
{
    public class BaseRepository<TModel> : Repository<User, TModel, int> where TModel : IModel<int>
    {
        public BaseRepository(SqlServerContext context) : base(context)
        {
        }

        protected override async Task<User> GetUserAsync(IDbConnection connection) => await Task.FromResult(new User()
        {
            Name = "adamo"
        });        

        protected override async Task BeforeSaveAsync(IDbConnection connection, SaveAction action, TModel model, IDbTransaction txn = null)
        {
            if (model is BaseTable baseTable)
            {
                switch (action)
                {
                    case SaveAction.Insert:
                        baseTable.CreatedBy = User.Name;
                        baseTable.DateCreated = User.LocalTime;
                        break;

                    case SaveAction.Update:
                        baseTable.ModifiedBy = User.Name;
                        baseTable.DateModified = User.LocalTime;
                        break;
                }
            }

            await Task.CompletedTask;
        }
    }
}
