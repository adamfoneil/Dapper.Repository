using AO.Models.Enums;
using Dapper.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dapper.Repository.MessageHandlers
{
    /// <summary>
    /// works for primary key and unique constraints
    /// </summary>
    public class DuplicateKeyError : IErrorMessageHandler
    {
        private readonly Func<string, string, string> _messageBuilder;

        public DuplicateKeyError(Func<string, string, string> messageBuilder)
        {
            _messageBuilder = messageBuilder;
        }

        public bool Filter(SaveAction action, Exception exception) => (exception is SqlException sqlException) ? 
            sqlException.Number == 2627 && action == SaveAction.Insert : false;
        
        public async Task<string> GetMessageAsync(IDbConnection connection, Exception exception)
        {
            var value = ParseDuplicateValue(exception.Message);
            var tableName = ParseTableName(exception.Message);
            var message = _messageBuilder.Invoke(value, tableName);
            return await Task.FromResult(message);
        }

        private string ParseDuplicateValue(string message)
        {
            var match = Regex.Match(message, @"duplicate key value is \((.*?)\)");
            
            if (match.Success) return match.Groups[1].Value;

            throw new Exception("Unable to parse duplicate key value");
        }

        private string ParseTableName(string message)
        {
            var match = Regex.Match(message, @"duplicate key in object '(.*?)'");
            
            if (match.Success) return match.Groups[1].Value;

            throw new Exception("Unable to parse table name");
        }
    }
}
