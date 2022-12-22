using AO.Models.Enums;
using Dapper.Repository.SqlServer.MessageHandlers;
using Microsoft.Data.SqlClient;
using System;

namespace Dapper.Repository.MessageHandlers
{
    public class DeleteCascadeBlocked : ForeignKeyError
    {
        public DeleteCascadeBlocked(Func<string, string, string> messageBuilder) : base(messageBuilder)
        {
        }

        public override bool Filter(SaveAction action, Exception exception) => 
            (exception is SqlException sqlEx) ?
                sqlEx.Number == FKError && action == SaveAction.Delete : false;
    }
}
