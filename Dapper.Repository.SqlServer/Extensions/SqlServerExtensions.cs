using AO.Models.Interfaces;
using Dapper.Repository.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository.SqlServer.Extensions
{
    public static class SqlServerExtensions
    {
        private const char StartDelimiter = '[';
        private const char EndDelimiter = ']';

        public static async Task<TModel> GetAsync<TModel, TKey>(this IDbConnection connection, TKey id, string identityColumn = CrudExtensionsBase.IdentityColumn) => 
            await CrudExtensionsBase.GetAsync<TModel, TKey>(connection, id, StartDelimiter, EndDelimiter, identityColumn);

        public static async Task<TModel> GetWhereAsync<TModel>(this IDbConnection connection, object criteria) =>
            await CrudExtensionsBase.GetWhereAsync<TModel>(connection, criteria, StartDelimiter, EndDelimiter);

        public static async Task<TModel> InsertAsync<TModel, TKey>(this IDbConnection connection, TModel model, IEnumerable<string> columnNames = null, string identityColumn = CrudExtensionsBase.IdentityColumn, Action<TModel, TKey> afterInsert = null) =>
            await CrudExtensionsBase.InsertAsync(connection, model, StartDelimiter, EndDelimiter, columnNames, identityColumn, "SELECT SCOPE_IDENTITY();", afterInsert);

        public static async Task UpdateAsync<TModel>(this IDbConnection connection, TModel model, IEnumerable<string> columnNames = null, string identityColumn = CrudExtensionsBase.IdentityColumn) =>
            await CrudExtensionsBase.UpdateAsync(connection, model, StartDelimiter, EndDelimiter, columnNames, identityColumn);

        public static async Task DeleteAsync<TModel, TKey>(this IDbConnection connection, TKey id, string identityColumn = CrudExtensionsBase.IdentityColumn, string tableName = null) =>
            await CrudExtensionsBase.DeleteAsync<TModel, TKey>(connection, id, StartDelimiter, EndDelimiter, identityColumn, tableName);

        public static async Task<TModel> SaveAynsc<TModel, TKey>(this IDbConnection connection, TModel model, IEnumerable<string> columnNames, string identityColumn = CrudExtensionsBase.IdentityColumn) where TModel : IModel<TKey>
        {
            if (model.Id.Equals(default(TKey)))
            {
                return await InsertAsync<TModel, TKey>(connection, model, columnNames, identityColumn, afterInsert: (row, id) => row.Id = id);
            }

            await UpdateAsync(connection, model, columnNames, identityColumn);
            return model;
        }
    }
}
