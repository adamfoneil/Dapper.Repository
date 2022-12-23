using AO.Models.Enums;
using Dapper.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dapper.Repository.SqlServer.MessageHandlers
{
    public abstract class ForeignKeyError : IErrorMessageHandler
    {
        private readonly Func<Info, string> _messageBuilder;

        protected const int ConstraintError = 547;

        public ForeignKeyError(Func<Info, string> messageBuilder)
        {
            _messageBuilder = messageBuilder;
        }

        public abstract bool Filter(SaveAction action, Exception exception);
        
        protected static bool IsFKError(Exception exception) => 
            (exception is SqlException sqlExc) ?
                sqlExc.Number == ConstraintError && (sqlExc.Message.Contains("REFERENCE constraint") || sqlExc.Message.Contains("FOREIGN KEY constraint")) : false;
        
        public async Task<string> GetMessageAsync(IDbConnection connection, Exception exception)
        {
            var fkName = ParseFKName(exception.Message);
            var fkInfo = await GetFKInfoAsync(connection, fkName);
            return _messageBuilder.Invoke(fkInfo);
        }

        private async Task<Info> GetFKInfoAsync(IDbConnection connection, string fkName)
        {
            var tables = await connection.QuerySingleAsync<(int ObjectId, string ReferencingName, string ReferencedName)>(
                @"SELECT 
	                [fk].[object_id], [referencing].[name] AS [ReferencingName], [referenced].[name] AS [ReferencedName]
                FROM 
	                [sys].[foreign_keys] [fk]
	                INNER JOIN [sys].[objects] [referencing] ON [fk].[parent_object_id]=[referencing].[object_id]
	                INNER JOIN [sys].[objects] [referenced] ON [fk].[referenced_object_id]=[referenced].[object_id]
                WHERE 
	                [fk].[name]=@fkName", new { fkName });

            var columns = await connection.QueryAsync<(string ReferencedName, string ReferencingName)>(
                @"SELECT	                    
                    [ref_col].[name] AS [ReferencedName],				
                    [child_col].[name] AS [ReferencingName]					
				FROM
					[sys].[foreign_key_columns] [fkcol]
					INNER JOIN [sys].[tables] [child_t] ON [fkcol].[parent_object_id]=[child_t].[object_id]
					INNER JOIN [sys].[columns] [child_col] ON
						[child_t].[object_id]=[child_col].[object_id] AND
						[fkcol].[parent_column_id]=[child_col].[column_id]
					INNER JOIN [sys].[tables] [ref_t] ON [fkcol].[referenced_object_id]=[ref_t].[object_id]
					INNER JOIN [sys].[columns] [ref_col] ON
						[ref_t].[object_id]=[ref_col].[object_id] AND
						[fkcol].[referenced_column_id]=[ref_col].[column_id]
                WHERE
                    [fkcol].[constraint_object_id]=@objectId", new { objectId = tables.ObjectId });

            return new Info()
            {
                ReferencedTable = tables.ReferencedName,
                ReferencingTable = tables.ReferencingName,
                Columns = columns
            };
        }            

        private string ParseFKName(string message)
        {
            var match = Regex.Match(message, @"constraint ""(.*?)""");

            if (match.Groups.Count > 0) return match.Groups[1].Value;

            throw new Exception($"Couldn't parse the FK name from message: {message}");
        }

        public class Info
        {
            /// <summary>
            /// parent or primary table
            /// </summary>
            public string ReferencedTable { get; init; }
            /// <summary>
            /// child or foreign key table
            /// </summary>
            public string ReferencingTable { get; init; }
            /// <summary>
            /// related columns
            /// </summary>
            public IEnumerable<(string ReferencedName, string ReferencingName)> Columns { get; init; }
        }
    }
}
