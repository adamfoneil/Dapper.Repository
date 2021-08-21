using Dapper.Repository.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository.SqlServer.Extensions
{
    public static class SqlServerExtensions
    {
        public static async Task<TModel> GetAsync<TModel, TKey>(this IDbConnection connection, TKey id, string identityColumn = CrudExtensionsBase.IdentityColumn) => 
            await CrudExtensionsBase.GetAsync<TModel, TKey>(connection, id, '[', ']', identityColumn);

        public static async Task<TModel> InsertAsync<TModel, TKey>(this IDbConnection connection, TModel model, IEnumerable<string> columnNames = null, string identityColumn = "Id", Action<TModel, TKey> afterInsert = null) =>
            await CrudExtensionsBase.InsertAsync(connection, model, '[', ']', columnNames, identityColumn, "SELECT SCOPE_IDENTITY();", afterInsert);

        public static async Task UpdateAsync<TModel>(this IDbConnection connection, TModel model, IEnumerable<string> columnNames = null, string identityColumn = CrudExtensionsBase.IdentityColumn) =>
            await CrudExtensionsBase.UpdateAsync(connection, model, '[', ']', columnNames, identityColumn);

        public static async Task DeleteAsync<TModel, TKey>(this IDbConnection connection, TKey id, string identityColumn = CrudExtensionsBase.IdentityColumn, string tableName = null) =>
            await CrudExtensionsBase.DeleteAsync<TModel, TKey>(connection, id, '[', ']', identityColumn, tableName);
    }
}
