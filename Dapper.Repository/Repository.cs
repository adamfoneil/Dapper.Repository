using AO.Models.Enums;
using AO.Models.Interfaces;
using AO.Models.Static;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository.Abstract
{
    public abstract class Repository<TKey>
    {
        protected abstract IDbConnection GetConnection();
        protected abstract char StartDelimiter { get; }
        protected abstract char EndDelimiter { get; }
        protected abstract string SelectIdentityCommand { get; }

        public async Task<TModel> GetAsync<TModel>(TKey id, IUserBase user = null, IDbTransaction txn = null) where TModel : IModel<TKey>
        {
            var cn = GetConnection();
            var sql = SqlBuilder.Get<TModel>(nameof(IModel<TKey>.Id), StartDelimiter, EndDelimiter);
            return await cn.QuerySingleOrDefaultAsync<TModel>(sql, new { id }, txn);
        }

        public async Task<TModel> GetWhereAsync<TModel>(object criteria, IUserBase user = null, IDbTransaction txn = null) where TModel : IModel<TKey>
        {
            throw new NotImplementedException();
        }

        public async Task<TModel> SaveAsync<TModel>(TModel model, IEnumerable<string> columnNames = null, IUserBase user = null, IDbTransaction txn = null) where TModel : IModel<TKey>
        {
            var action = IsInsert(model) ? SaveAction.Insert : SaveAction.Update;

            var sql =
                (action == SaveAction.Insert) ? SqlBuilder.Insert<TModel>(columnNames, StartDelimiter, EndDelimiter) + " " + SelectIdentityCommand :
                (action == SaveAction.Update) ? SqlBuilder.Update<TModel>(columnNames, StartDelimiter, EndDelimiter) :
                throw new Exception($"Unrecognized save action: {action}");

            var cn = GetConnection();
            var result = await cn.QuerySingleOrDefaultAsync<TKey>(sql, model, txn);

            if (action == SaveAction.Insert) model.Id = result;

            return model;
        }

        private bool IsInsert<TModel>(TModel model) where TModel : IModel<TKey> => model.Id.Equals(default);
        

        public async Task DeleteAsync<TModel>(TModel model, string userName = null) where TModel : IModel<TKey>
        {

        }
    }
}
