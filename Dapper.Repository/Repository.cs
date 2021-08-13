using AO.Models.Enums;
using AO.Models.Interfaces;
using AO.Models.Static;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository.Abstract
{
    public class Repository<TModel, TKey> where TModel : IModel<TKey>
    {
        private readonly DbContext _context;

        public Repository(DbContext context)
        {
            _context = context;
        }

        public async virtual Task<TModel> GetAsync(TKey id, IDbTransaction txn = null)
        {
            var cn = _context.GetConnection();
            var sql = SqlBuilder.Get<TModel>(nameof(IModel<TKey>.Id), _context.StartDelimiter, _context.EndDelimiter);
            var result = await cn.QuerySingleOrDefaultAsync<TModel>(sql, new { id }, txn);
            await GetRelatedAsync(cn, result, txn);
            return result;
        }

        /// <summary>
        /// override this to populate "navigation properties" of your model row
        /// </summary>
        protected async virtual Task GetRelatedAsync(IDbConnection connection, TModel model, IDbTransaction txn = null) => await Task.CompletedTask;

        public async virtual Task<TModel> GetWhereAsync(object criteria, IDbTransaction txn = null)
        {
            throw new NotImplementedException();
        }

        public async virtual Task<TModel> SaveAsync(TModel model, IEnumerable<string> columnNames = null, IDbTransaction txn = null)
        {
            var action = GetSaveAction(model);

            var sql =
                (action == SaveAction.Insert) ? SqlBuilder.Insert<TModel>(columnNames, _context.StartDelimiter, _context.EndDelimiter) + " " + _context.SelectIdentityCommand :
                (action == SaveAction.Update) ? SqlBuilder.Update<TModel>(columnNames, _context.StartDelimiter, _context.EndDelimiter) :
                throw new Exception($"Unrecognized save action: {action}");

            var cn = _context.GetConnection();            
            var result = await cn.QuerySingleOrDefaultAsync<TKey>(sql, model, txn);

            if (action == SaveAction.Insert) model.Id = result;

            return model;
        }
        
        public async virtual Task DeleteAsync(TModel model) 
        {

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
    }
}
