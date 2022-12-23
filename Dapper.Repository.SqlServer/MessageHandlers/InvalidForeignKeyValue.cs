using AO.Models.Enums;
using Microsoft.Data.SqlClient;
using System;

namespace Dapper.Repository.SqlServer.MessageHandlers
{
    public class InvalidForeignKeyValue : ForeignKeyError
    {
        public InvalidForeignKeyValue(Func<Info, string> messageBuilder) : base(messageBuilder)
        {
        }

        public override bool Filter(SaveAction action, Exception exception) =>
            IsFKError(exception) && (action == SaveAction.Insert || action == SaveAction.Update);
    }
}
