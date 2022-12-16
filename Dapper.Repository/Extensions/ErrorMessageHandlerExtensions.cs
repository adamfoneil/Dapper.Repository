﻿using Dapper.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.Repository.Extensions
{
    public static class ErrorMessageHandlerExtensions
    {
        public static async Task<string> GetErrorMessageAsync(this IEnumerable<IErrorMessageHandler> handlers, IDbConnection connection, Exception exception)
        {
            foreach (var item in handlers)
            {
                if (item.Filter(exception))
                {
                    return await item.GetMessageAsync(connection, exception);
                }
            }

            return exception.Message;
        }
    }
}
