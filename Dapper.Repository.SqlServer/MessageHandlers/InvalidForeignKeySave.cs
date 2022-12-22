using AO.Models.Enums;
using Microsoft.Data.SqlClient;
using System;

namespace Dapper.Repository.SqlServer.MessageHandlers
{
    public class InvalidForeignKeySave : ForeignKeyError
    {
        public InvalidForeignKeySave(Func<string, string, string> messageBuilder) : base(messageBuilder)
        {
        }

        public override bool Filter(SaveAction action, Exception exception) => 
            (exception is SqlException sqlEx) ?
                sqlEx.Number == FKError && (action == SaveAction.Insert || action == SaveAction.Update) : false;
    }
}
