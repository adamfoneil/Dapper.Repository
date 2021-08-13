using AO.Models.Enums;
using AO.Models.Interfaces;
using AO.Models.Static;
using Dapper.Repository.Exceptions;
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
    public partial class Repository<TModel, TKey> where TModel : IModel<TKey>
    {
        protected readonly DbContext Context;
        protected readonly ILogger Logger;

        public Repository(DbContext context)
        {
            Context = context;
            Logger = context.Logger;
        }

        public async virtual Task<TModel> GetAsync(TKey id, IDbTransaction txn = null)
        {
            var cn = Context.GetConnection();
            var sql = SqlBuilder.Get<TModel>(nameof(IModel<TKey>.Id), Context.StartDelimiter, Context.EndDelimiter);

            TModel result;

            try
            {
                result = await cn.QuerySingleOrDefaultAsync<TModel>(sql, new { id }, txn);
            }
            catch (Exception exc)
            {
                var sqlExc = GetSqlException(exc, sql, new { id });
                throw sqlExc;
            }
            
            var allow = await AllowGetAsync(cn, result, txn);
            if (!allow.result) throw new PermissionException($"Get permission was denied: {allow.message}");

            await GetRelatedAsync(cn, result, txn);

            return result;       
        }

        public async virtual Task<TModel> GetWhereAsync(object criteria, IDbTransaction txn = null)
        {
            throw new NotImplementedException();
        }

        public async virtual Task<TModel> SaveAsync(TModel model, IEnumerable<string> columnNames = null, IDbTransaction txn = null)
        {
            var action = GetSaveAction(model);
            
            var sql =
                (action == SaveAction.Insert) ? SqlBuilder.Insert<TModel>(columnNames, Context.StartDelimiter, Context.EndDelimiter) + " " + Context.SelectIdentityCommand :
                (action == SaveAction.Update) ? SqlBuilder.Update<TModel>(columnNames, Context.StartDelimiter, Context.EndDelimiter) :
                throw new Exception($"Unrecognized save action: {action}");

            var cn = Context.GetConnection();
            var validation = await ValidateAsync(cn, model, txn);
            if (!validation.result) throw new ValidationException(validation.message);

            var result = default(TKey);

            try
            {
                result = await cn.QuerySingleOrDefaultAsync<TKey>(sql, model, txn);
            }
            catch (Exception exc)
            {
                var sqlExc = GetSqlException(exc, sql, model);
                throw sqlExc;
            }

            if (action == SaveAction.Insert) model.Id = result;

            await AfterSaveAsync(cn, action, model, txn);

            return model;
        }
        
        public async virtual Task DeleteAsync(TModel model, IDbTransaction txn = null) 
        {
            var cn = Context.GetConnection();

            var allow = await AllowDeleteAsync(cn, model, txn);
            if (!allow.result) throw new PermissionException($"Delete permission was denied: {allow.message}");

            await BeforeDeleteAsync(cn, model, txn);

            var sql = SqlBuilder.Delete<TModel>(Context.StartDelimiter, Context.EndDelimiter);

            try
            {
                await cn.ExecuteAsync(sql, new { id = model.Id }, txn);
            }
            catch (Exception exc)
            {
                var sqlExc = GetSqlException(exc, sql, model);
                throw sqlExc;
            }

            await AfterDeleteAsync(cn, model, txn);
        }

        public async virtual Task<TKey> MergeAsync(TModel model)
        {
            if (IsNew(model))
            {

            }

            throw new NotImplementedException();
        }        

        private bool IsNew(TModel model) => model.Id.Equals(default);

        private SaveAction GetSaveAction(TModel model) => IsNew(model) ? SaveAction.Insert : SaveAction.Update;

        private SqlException GetSqlException(Exception exception, string sql, object model, [CallerMemberName] string methodName = null)
        {
            Logger?.LogError(exception, JsonSerializer.Serialize(new
            {
                message = exception.Message,
                sql = sql,
                parameters = model,
                methodName
            }));

            return new SqlException(exception.Message, sql, model);
        }
    }
}
