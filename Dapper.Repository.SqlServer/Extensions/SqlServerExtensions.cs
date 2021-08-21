using Dapper.Repository.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository.SqlServer.Extensions
{
    public static class SqlServerExtensions
    {
        public static async Task<TModel> GetAsync<TModel, TKey>(this IDbConnection connection, TKey id, string identityColumn = "Id") => 
            await CrudExtensionsBase.GetAsync<TModel, TKey>(connection, id, '[', ']', identityColumn);

        public static async Task<TModel> InsertAsync<TModel>(this IDbConnection connection, TModel model, IEnumerable<string> columnNames = null, string identityColumn = "Id", Func<IDbConnection, TModel, Task> afterInsert = null) =>
            await CrudExtensionsBase.InsertAsync(connection, model, '[', ']', columnNames, identityColumn, afterInsert);

        public static async Task UpdateAsync<TModel>(this IDbConnection connection, TModel model, IEnumerable<string> columnNames = null, string identityColumn = "Id") =>
            await CrudExtensionsBase.UpdateAsync(connection, model, '[', ']', columnNames, identityColumn);
    }
}
