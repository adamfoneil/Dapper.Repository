using AO.Models.Static;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Repository.Extensions
{
    internal static partial class CrudExtensionsBase
    {
        internal static async Task<object> InsertAsync(this IDbConnection connection,
            string tableName, Dictionary<string, object> columnValues, char startDelimiter, char endDelimiter,
            string selectIdentityCommand = null, IDbTransaction txn = null)
        {
            var sql = SqlBuilder.Insert(tableName, columnValues.Keys, startDelimiter, endDelimiter) + $" {selectIdentityCommand}";

            var dp = new DynamicParameters();
            foreach (var kp in columnValues) dp.Add(kp.Key, kp.Value);

            return await connection.ExecuteScalarAsync(sql, dp, txn);            
        }

        internal static async Task UpdateAsync(this IDbConnection connection,
            string tableName, Dictionary<string, object> columnValues, char startDelimiter, char endDelimiter, 
            string identityColumn = IdentityColumn, IDbTransaction txn = null)
        {
            var sql = SqlBuilder.Update(tableName, columnValues.Keys.Except(new [] { identityColumn }), startDelimiter, endDelimiter, identityColumn);

            var dp = new DynamicParameters();
            foreach (var kp in columnValues) dp.Add(kp.Key, kp.Value);

            try
            {
                dp.Add(identityColumn, columnValues[identityColumn]);
            }
            catch (Exception exc)
            {
                throw new Exception("Error building UPDATE statement. Make sure you pass the identity column and value in your dictionary. " + exc.Message, exc);
            }
            
            await connection.ExecuteAsync(sql, dp, txn);
        }
    }
}
