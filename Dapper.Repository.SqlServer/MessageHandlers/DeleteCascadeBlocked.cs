using AO.Models.Enums;
using Dapper.Repository.SqlServer.MessageHandlers;
using System;

namespace Dapper.Repository.MessageHandlers
{
    public class DeleteCascadeBlocked : ForeignKeyError
    {
        public DeleteCascadeBlocked(Func<Info, string> messageBuilder) : base(messageBuilder)
        {
        }

        public override bool Filter(SaveAction action, Exception exception) => IsFKError(exception) && action == SaveAction.Delete;
    }
}
