﻿using AO.Models.Enums;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository
{
    public partial class Repository<TContext, TUser, TModel, TKey>
    {
        protected virtual string SqlGet { get; }

        protected virtual CommandType SqlGetCommandType => CommandType.Text;

        protected virtual string SqlDelete { get; }

        protected virtual CommandType SqlDeleteCommandType => CommandType.Text;

        protected virtual string SqlGetWhere { get; }

        protected virtual CommandType SqlGetWhereCommandType => CommandType.Text;

        protected virtual object SqlIdParameter(TKey id) => new { id };

        protected virtual object SqlIdDeleteParameter(TKey id) => null;

        /// <summary>
        /// override this to populate "navigation properties" of your model row
        /// </summary>
        protected async virtual Task GetRelatedAsync(IDbConnection connection, TModel model, IDbTransaction txn = null) => await Task.CompletedTask;

        /// <summary>
        /// override this to implement a permission check on a GetAsync, GetWhereAsync, or MergeAsync
        /// </summary>
        protected async virtual Task<(bool result, string message)> AllowGetAsync(IDbConnection connection, TModel model, IDbTransaction txn = null)
        {
            await Task.CompletedTask;
            return (true, null);
        }

        /// <summary>
        /// override this to check user's delete permission on a model
        /// </summary>
        protected async virtual Task<(bool result, string message)> AllowDeleteAsync(IDbConnection connection, TModel model, IDbTransaction txn = null)
        {
            await Task.CompletedTask;
            return (true, null);
        }

        /// <summary>
        /// override this to validate a model prior to saving
        /// </summary>
        protected async virtual Task<(bool result, string message)> ValidateAsync(IDbConnection connection, SaveAction action, TModel model, IDbTransaction txn = null)
        {
            await Task.CompletedTask;
            return (true, null);
        }

        protected async virtual Task BeforeSaveAsync(IDbConnection connection, SaveAction action, TModel model, IDbTransaction txn = null) => await Task.CompletedTask;

        protected async virtual Task AfterSaveAsync(IDbConnection connection, SaveAction action, TModel model, IDbTransaction txn = null) => await Task.CompletedTask;

        protected async virtual Task BeforeDeleteAsync(IDbConnection connection, TModel model, IDbTransaction txn = null) => await Task.CompletedTask;

        protected async virtual Task AfterDeleteAsync(IDbConnection connection, TModel model, IDbTransaction txn = null) => await Task.CompletedTask;

        protected virtual void SetIdentity(TKey id, TModel model) => model.Id = id;

        protected virtual string AdditionalLogData => string.Empty;
    }
}
