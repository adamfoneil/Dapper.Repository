using AO.Models.Enums;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository.Interfaces
{
    public interface IErrorMessageHandler
    {
        bool Filter(SaveAction action, Exception exception);
        Task<string> GetMessageAsync(IDbConnection connection, Exception exception);
    }
}
