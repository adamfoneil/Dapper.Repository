using AO.Models.Enums;
using AO.Models.Interfaces;
using AO.Models.Static;
using Dapper.Repository.Exceptions;
using Dapper.Repository.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dapper.Repository
{
    public partial class Repository<TUser, TModel, TKey> where TModel : IModel<TKey>
    {
        protected readonly DbContext<TUser> Context;
        protected readonly ILogger Logger;

        public Repository(DbContext<TUser> context)
        {
            Context = context;
            Logger = context.Logger;
        }        

        public async virtual Task<TModel> GetAsync(TKey id, IDbTransaction txn = null)
        {
            var sql = SqlGet ?? SqlBuilder.Get<TModel>(Context.StartDelimiter, Context.EndDelimiter);
            return await GetInnerAsync(sql, SqlIdParameter(id), SqlGetCommandType, txn);
        }

        public async virtual Task<TModel> GetWhereAsync(object criteria, IDbTransaction txn = null)
        {
            var sql = SqlGetWhere ?? SqlBuilder.GetWhere<TModel>(criteria, Context.StartDelimiter, Context.EndDelimiter);
            return await GetInnerAsync(sql, criteria, SqlGetWhereCommandType, txn);
        }

        public async virtual Task<TModel> SaveAsync(TModel model, IEnumerable<string> columnNames = null, IDbTransaction txn = null)
        {
            await Context.GetUserAsync();

            var action = GetSaveAction(model);

            var sql =
                (action == SaveAction.Insert) ? SqlBuilder.Insert<TModel>(columnNames, Context.StartDelimiter, Context.EndDelimiter) + " " + Context.SelectIdentityCommand :
                (action == SaveAction.Update) ? SqlBuilder.Update<TModel>(columnNames, Context.StartDelimiter, Context.EndDelimiter) :
                throw new Exception($"Unrecognized save action: {action}");

            var cn = Context.GetConnection();
            
            var validation = await ValidateAsync(cn, action, model, txn);
            if (!validation.result) throw new ValidationException(validation.message);

            await BeforeSaveAsync(cn, action, model);

            TKey result;
            try
            {
                result = await cn.QuerySingleOrDefaultAsync<TKey>(sql, model, txn);
            }
            catch (Exception exc)
            {
                throw GetSqlException(exc, sql, model);
            }

            if (action == SaveAction.Insert) SetIdentity(result, model);

            await AfterSaveAsync(cn, action, model, txn);

            return model;
        }

        public async virtual Task DeleteAsync(TModel model, IDbTransaction txn = null)
        {
            await Context.GetUserAsync();

            var cn = Context.GetConnection();
            
            var allow = await AllowDeleteAsync(cn, model, txn);
            if (!allow.result) throw new PermissionException($"Delete permission was denied: {allow.message}");

            await BeforeDeleteAsync(cn, model, txn);

            var sql = SqlDelete ?? SqlBuilder.Delete<TModel>(Context.StartDelimiter, Context.EndDelimiter);

            try
            {
                await cn.ExecuteAsync(sql, SqlIdParameter(model.Id), commandType: SqlDeleteCommandType, transaction: txn);
            }
            catch (Exception exc)
            {
                throw GetSqlException(exc, sql, model);
            }

            await AfterDeleteAsync(cn, model, txn);
        }

        public async virtual Task<TModel> MergeAsync(TModel model, Action<TModel, TModel> onExisting = null, IDbTransaction txn = null)
        {
            TModel existing;
            if (IsNew(model))
            {
                var sql = CrudExtensionsBase.BuildMergeGetCommand(model, Context.StartDelimiter, Context.EndDelimiter);
                existing = await GetInnerAsync(sql, model, CommandType.Text, txn);
                if (existing != null)
                {
                    model.Id = existing.Id;
                    onExisting?.Invoke(model, existing);
                }
            }

            return await SaveAsync(model, txn: txn);
        }

        protected virtual bool IsNew(TModel model) => model.Id.Equals(default(TKey));

        private SaveAction GetSaveAction(TModel model) => IsNew(model) ? SaveAction.Insert : SaveAction.Update;

        private QueryException GetSqlException(Exception exception, string sql, object model, [CallerMemberName] string methodName = null)
        {
            Logger?.LogError(exception, JsonSerializer.Serialize(new
            {
                message = exception.Message,
                sql = sql,
                parameters = model,
                methodName
            }));

            return new QueryException(exception.Message, sql, model);
        }

        private async Task<TModel> GetInnerAsync(string sql, object parameters, CommandType commandType, IDbTransaction txn = null)
        {
            await Context.GetUserAsync();

            var cn = Context.GetConnection();
            
            TModel result;

            try
            {
                result = await cn.QuerySingleOrDefaultAsync<TModel>(sql, parameters, commandType: commandType, transaction: txn);
            }
            catch (Exception exc)
            {
                throw GetSqlException(exc, sql, parameters);                
            }

            var allow = await AllowGetAsync(cn, result, txn);
            if (!allow.result) throw new PermissionException($"Get permission was denied: {allow.message}");

            if (result is not null)
            {
                await GetRelatedAsync(cn, result, txn);
            }            

            return result;
        }
    }
}
