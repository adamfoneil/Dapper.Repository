using AO.Models.Static;
using Dapper.Repository.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dapper.Repository.Extensions
{
    internal static partial class CrudExtensionsBase
    {
        public const string IdentityColumn = "Id";

        internal static async Task<TModel> GetAsync<TModel, TKey>(this IDbConnection connection, TKey id, char startDelimiter, char endDelimiter, string identityColumn = IdentityColumn, IDbTransaction txn = null)
        {
            var sql = SqlBuilder.Get<TModel>(startDelimiter, endDelimiter, identityColumn);

            try
            {
                var param = new DynamicParameters();
                param.Add(identityColumn, id);
                return await connection.QuerySingleOrDefaultAsync<TModel>(sql, param, txn);
            }
            catch (Exception exc)
            {
                throw new RepositoryException(exc.Message, sql);
            }            
        }

        internal static async Task<TModel> GetWhereAsync<TModel>(this IDbConnection connection, object criteria, char startDelimiter, char endDelimiter, IDbTransaction txn = null)
        {
            var sql = SqlBuilder.GetWhere<TModel>(criteria, startDelimiter, endDelimiter);

            try
            {
                return await connection.QuerySingleOrDefaultAsync<TModel>(sql, criteria, txn);
            }
            catch (Exception exc)
            {
                throw new RepositoryException(exc.Message, sql);
            }
        }

        internal static async Task<TModel> InsertAsync<TModel, TKey>(this IDbConnection connection, TModel model, char startDelimiter, char endDelimiter, IEnumerable<string> columnNames = null, string identityColumn = IdentityColumn, string selectIdentityCommand = null, Action<TModel, TKey> afterInsert = null, IDbTransaction txn = null)
        {
            var sql = SqlBuilder.Insert<TModel>(columnNames, startDelimiter: startDelimiter, endDelimiter: endDelimiter, identityColumn: identityColumn);
            sql += $"; {selectIdentityCommand}";

            try
            {
                var id = await connection.QuerySingleOrDefaultAsync<TKey>(sql, model, txn);
                afterInsert?.Invoke(model, id);
            }
            catch (Exception exc)
            {
                throw new RepositoryException(exc.Message, sql, model);
            }

            return model;                        
        }

        internal static async Task UpdateAsync<TModel>(this IDbConnection connection, TModel model, char startDelimiter, char endDelimiter, IEnumerable<string> columnNames, string identityColumn = IdentityColumn, IDbTransaction txn = null)
        {
            var sql = SqlBuilder.Update<TModel>(columnNames, startDelimiter: startDelimiter, endDelimiter: endDelimiter, identityColumn: identityColumn);

            try
            {
                await connection.ExecuteAsync(sql, model, txn);
            }
            catch (Exception exc)
            {
                throw new RepositoryException(exc.Message, sql, model);
            }            
        }

        internal static async Task DeleteAsync<TModel, TKey>(this IDbConnection connection, TKey id, char startDelimiter, char endDelimiter, string identityColumn = IdentityColumn, string tableName = null, IDbTransaction txn = null)
        {
            var sql = SqlBuilder.Delete<TModel>(startDelimiter, endDelimiter, identityColumn, tableName);

            try
            {
                var param = new DynamicParameters();
                param.Add(identityColumn, id);
                await connection.ExecuteAsync(sql, param, txn);
            }
            catch (Exception exc)
            {
                throw new RepositoryException(exc.Message, sql);
            }            
        }            

        internal static string BuildMergeGetCommand<TModel>(TModel model, char startDelimiter, char endDelimiter)
        {
            var keyProperties = GetKeyProperties(model).Select(pi => pi.Name);
            if (!keyProperties.Any()) throw new Exception($"To use MergeAsync with type {typeof(TModel).Name}, it must have at least one property with the [Key] attribute.");
            return SqlBuilder.GetWhere(typeof(TModel), keyProperties, startDelimiter, endDelimiter);
        }

        private static IEnumerable<PropertyInfo> GetKeyProperties<TModel>(TModel model) =>
            model.GetType().GetProperties().Where(pi =>
            {
                var attr = pi.GetCustomAttribute(typeof(KeyAttribute));
                return attr != null;
            });
    }    
}
