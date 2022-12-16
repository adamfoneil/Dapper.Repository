﻿using Dapper.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dapper.Repository.MessageHandlers
{
    public class ForeignKeyHandler : IErrorMessageHandler
    {
        private readonly Func<string, string, string> _messageBuilder;

        public ForeignKeyHandler(Func<string, string, string> messageBuilder)
        {
            _messageBuilder = messageBuilder;
        }

        public bool Filter(Exception exception) => (exception is SqlException sqlEx) ? sqlEx.Number == 547 : false;

        public async Task<string> GetMessageAsync(IDbConnection connection, Exception exception)
        {
            var fkName = ParseFKName(exception.Message);
            var fkInfo = await GetFKInfoAsync(connection, fkName);
            return _messageBuilder.Invoke(fkInfo.ReferencedTable, fkInfo.ReferencingTable);            
        }

        private async Task<(string ReferencingTable, string ReferencedTable)> GetFKInfoAsync(IDbConnection connection, string fkName) =>
            await connection.QuerySingleAsync<(string ReferencingName, string ReferencedName)>(
                @"SELECT 
	                [referencing].[name] AS [ReferencingName], [referenced].[name] AS [ReferencedName]
                FROM 
	                [sys].[foreign_keys] [fk]
	                INNER JOIN [sys].[objects] [referencing] ON [fk].[parent_object_id]=[referencing].[object_id]
	                INNER JOIN [sys].[objects] [referenced] ON [fk].[referenced_object_id]=[referenced].[object_id]
                WHERE 
	                [fk].[name]=@fkName", new { fkName });

        private string ParseFKName(string message)
        {
            var match = Regex.Match(message, @"constraint ""(.*?)""");

            if (match.Groups.Count > 0) return match.Groups[1].Value;

            throw new Exception($"Couldn't parse the FK name from message: {message}");
        }
    }
}
