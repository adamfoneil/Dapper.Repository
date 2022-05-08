using Dapper.Repository.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository.SqlServer.Extensions
{
    public static partial class SqlServerExtensions
    {
        public static async Task<object> InsertAsync(this IDbConnection connection,
            string tableName, Dictionary<string, object> columnValues, IDbTransaction txn = null) =>
            await CrudExtensionsBase.InsertAsync(
                connection, tableName, columnValues, StartDelimiter, EndDelimiter,
                "SELECT SCOPE_IDENTITY()", txn);

        public static async Task UpdateAsync(this IDbConnection connection,
            string tableName, Dictionary<string, object> columnValues, string identityColumn = CrudExtensionsBase.IdentityColumn, IDbTransaction txn = null) =>
            await CrudExtensionsBase.UpdateAsync(
                connection, tableName, columnValues, StartDelimiter, EndDelimiter,
                identityColumn, txn);
    }
}
