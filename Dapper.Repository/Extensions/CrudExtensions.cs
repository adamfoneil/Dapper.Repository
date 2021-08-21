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
        public static async Task<TModel> GetAsync<TModel, TKey>(this IDbConnection connection, TKey id, char startDelimiter, char endDelimiter, string identityColumn = "Id")
        {
            var sql = SqlBuilder.Get<TModel>(startDelimiter, endDelimiter, identityColumn);
            return await connection.QuerySingleOrDefaultAsync<TModel>(sql, new { id });
        }

        public static async Task<TModel> InsertAsync<TModel>(this IDbConnection connection, TModel model, char startDelimiter, char endDelimiter, IEnumerable<string> columnNames = null, string identityColumn = "Id", Func<IDbConnection, TModel, Task> afterInsert = null)
        {
            var sql = SqlBuilder.Insert<TModel>(columnNames, startDelimiter: startDelimiter, endDelimiter: endDelimiter, identityColumn: identityColumn);

            try
            {
                await connection.ExecuteAsync(sql, model);
            }
            catch (Exception exc)
            {
                throw new SqlException(exc.Message, sql, model);
            }

            // intended for getting identity value
            if (afterInsert != null) await afterInsert.Invoke(connection, model);
            return model;                        
        }

        public static async Task UpdateAsync<TModel>(this IDbConnection connection, TModel model, char startDelimiter, char endDelimiter, IEnumerable<string> columnNames, string identityColumn = "Id")
        {
            var sql = SqlBuilder.Update<TModel>(columnNames, startDelimiter: startDelimiter, endDelimiter: endDelimiter, identityColumn: identityColumn);
            await connection.ExecuteAsync(sql, model);            
        }
    }
}
