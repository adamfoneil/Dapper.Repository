using AO.Models.Static;
using Dapper.Repository.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository.Extensions
{
    internal static class CrudExtensionsBase
    {
        public const string IdentityColumn = "Id";

        internal static async Task<TModel> GetAsync<TModel, TKey>(this IDbConnection connection, TKey id, char startDelimiter, char endDelimiter, string identityColumn = IdentityColumn)
        {
            var sql = SqlBuilder.Get<TModel>(startDelimiter, endDelimiter, identityColumn);

            try
            {
                var param = new DynamicParameters();
                param.Add(identityColumn, id);
                return await connection.QuerySingleOrDefaultAsync<TModel>(sql, param);
            }
            catch (Exception exc)
            {
                throw new SqlException(exc.Message, sql);
            }            
        }

        internal static async Task<TModel> InsertAsync<TModel, TKey>(this IDbConnection connection, TModel model, char startDelimiter, char endDelimiter, IEnumerable<string> columnNames = null, string identityColumn = IdentityColumn, string selectIdentityCommand = null, Action<TModel, TKey> afterInsert = null)
        {
            var sql = SqlBuilder.Insert<TModel>(columnNames, startDelimiter: startDelimiter, endDelimiter: endDelimiter, identityColumn: identityColumn);
            sql += $"; {selectIdentityCommand}";

            try
            {
                var id = await connection.QuerySingleOrDefaultAsync<TKey>(sql, model);
                afterInsert?.Invoke(model, id);
            }
            catch (Exception exc)
            {
                throw new SqlException(exc.Message, sql, model);
            }

            return model;                        
        }

        internal static async Task UpdateAsync<TModel>(this IDbConnection connection, TModel model, char startDelimiter, char endDelimiter, IEnumerable<string> columnNames, string identityColumn = IdentityColumn)
        {
            var sql = SqlBuilder.Update<TModel>(columnNames, startDelimiter: startDelimiter, endDelimiter: endDelimiter, identityColumn: identityColumn);

            try
            {
                await connection.ExecuteAsync(sql, model);
            }
            catch (Exception exc)
            {
                throw new SqlException(exc.Message, sql, model);
            }            
        }

        internal static async Task DeleteAsync<TModel, TKey>(this IDbConnection connection, TKey id, char startDelimiter, char endDelimiter, string identityColumn = IdentityColumn, string tableName = null)
        {
            var sql = SqlBuilder.Delete<TModel>(startDelimiter, endDelimiter, identityColumn, tableName);

            try
            {
                var param = new DynamicParameters();
                param.Add(identityColumn, id);
                await connection.ExecuteAsync(sql, param);
            }
            catch (Exception exc)
            {
                throw new SqlException(exc.Message, sql);
            }            
        }            
    }
}
