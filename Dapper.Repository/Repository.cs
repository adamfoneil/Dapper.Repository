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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dapper.Repository
{
    public partial class Repository<TContext, TUser, TModel, TKey> 
        where TModel : IModel<TKey>
        where TContext : DbContext<TUser>
    {
        protected readonly TContext Context;
        protected readonly ILogger Logger;

        public Repository(TContext context)
        {
            Context = context;
            Logger = context.Logger;
        }

        public async virtual Task<TModel> GetAsync(TKey id, IDbTransaction txn = null, [CallerMemberName]string methodName = null)
        {
            var sql = SqlGet ?? SqlBuilder.Get<TModel>(Context.StartDelimiter, Context.EndDelimiter);
            return await GetInnerAsync(sql, SqlIdParameter(id), SqlGetCommandType, methodName, txn);
        }

        public async virtual Task<TModel> GetWhereAsync(object criteria, IDbTransaction txn = null, [CallerMemberName]string methodName = null)
        {
            var sql = SqlGetWhere ?? SqlBuilder.GetWhere<TModel>(criteria, Context.StartDelimiter, Context.EndDelimiter);
            return await GetInnerAsync(sql, criteria, SqlGetWhereCommandType, methodName, txn);
        }

        public async virtual Task<TModel> SaveAsync(TModel model, IEnumerable<string> columnNames = null, IDbTransaction txn = null, [CallerMemberName]string methodName = null)
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
                Logger.LogTrace("{methodName}:\r\n{sql}", methodName, sql);
                if (Logger.IsEnabled(LogLevel.Trace)) Logger.LogTrace("{methodName}: params {model}", methodName, JsonSerializer.Serialize(model));
                
                var sw = Stopwatch.StartNew();
                result = await cn.QuerySingleOrDefaultAsync<TKey>(sql, model, txn);
                LogElapsed(methodName, sw);
            }
            catch (Exception exc)
            {
                var message = await Context.MessageHandlers.GetErrorMessageAsync(cn, action, exc);
                throw LogAndGetException(exc, message, sql, model);
            }

            if (action == SaveAction.Insert) SetIdentity(result, model);

            await AfterSaveAsync(cn, action, model, txn);

            return model;
        }

        public async virtual Task DeleteAsync(TModel model, IDbTransaction txn = null, [CallerMemberName]string methodName = null)
        {
            await Context.GetUserAsync();

            var cn = Context.GetConnection();
            
            var allow = await AllowDeleteAsync(cn, model, txn);
            if (!allow.result) throw new PermissionException($"Delete permission was denied: {allow.message}");

            await BeforeDeleteAsync(cn, model, txn);

            var sql = SqlDelete ?? SqlBuilder.Delete<TModel>(Context.StartDelimiter, Context.EndDelimiter);

            try
            {
                var param = SqlIdDeleteParameter(model.Id) ?? SqlIdParameter(model.Id);
                Logger.LogTrace("{methodName}:\r\n{sql} with {param}", methodName, sql, param);

                var sw = Stopwatch.StartNew();
                await cn.ExecuteAsync(sql, param, commandType: SqlDeleteCommandType, transaction: txn);
                LogElapsed(methodName, sw);
            }
            catch (Exception exc)
            {
                var message = await Context.MessageHandlers.GetErrorMessageAsync(cn, SaveAction.Delete, exc);
                throw LogAndGetException(exc, message, sql, model);
            }

            await AfterDeleteAsync(cn, model, txn);
        }

        public async virtual Task<TModel> MergeAsync(TModel model, Action<TModel, TModel> onExisting = null, IDbTransaction txn = null, [CallerMemberName]string methodName = null)
        {
            TModel existing;
            if (IsNew(model))
            {
                var sql = CrudExtensionsBase.BuildMergeGetCommand(model, Context.StartDelimiter, Context.EndDelimiter);
                existing = await GetInnerAsync(sql, model, CommandType.Text, methodName, txn);
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

        private RepositoryException LogAndGetException(Exception innerException, string message, string sql, object model, [CallerMemberName] string methodName = null)
        {
            var result = new RepositoryException(message, sql, model, innerException);
            
            Logger?.LogError(result, message);

            return result;
        }

        private async Task<TModel> GetInnerAsync(string sql, object parameters, CommandType commandType, string methodName, IDbTransaction txn = null)
        {
            await Context.GetUserAsync();

            var cn = Context.GetConnection();
            
            TModel result;
            
            var sw = Stopwatch.StartNew();

            try
            {
                Logger.LogTrace("{methodName}:\r\n{sql} with {params}", methodName, sql, parameters);
                result = await cn.QuerySingleOrDefaultAsync<TModel>(sql, parameters, commandType: commandType, transaction: txn);

                LogElapsed(methodName, sw);
            }
            catch (Exception exc)
            {
                var message = await Context.MessageHandlers.GetErrorMessageAsync(cn, SaveAction.None, exc);
                throw LogAndGetException(exc, message, sql, parameters);                
            }

            var allow = await AllowGetAsync(cn, result, txn);
            if (!allow.result) throw new PermissionException($"Get permission was denied: {allow.message}");

            if (result is not null)
            {
                await GetRelatedAsync(cn, result, txn);
            }            

            return result;
        }

        private void LogElapsed(string methodName, Stopwatch sw)
        {
            sw.Stop();
            Logger.LogTrace("{methodName}: elapsed {elapsed}", methodName, sw.Elapsed);
        }
    }
}
