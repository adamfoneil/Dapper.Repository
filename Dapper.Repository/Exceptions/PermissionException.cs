using System;

namespace Dapper.Repository.Exceptions
{
    public class PermissionException : Exception
    {
        public PermissionException(string message) : base(message)
        {
        }
    }
}
