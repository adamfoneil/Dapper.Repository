using AO.Models.Interfaces;
using Dapper.Repository.SqlServer;
using Microsoft.Extensions.Logging;
using SqlServer.LocalDb;
using System;

namespace Dapper.Repository.Test
{
    public class User : IUserBase
    {
        public string Name { get; set; }
        public DateTime LocalTime => DateTime.UtcNow;
    }

    public class MyContext : SqlServerContext
    {
        public const string DbName = "DapperRepository";

        public MyContext(ILogger logger) : base(LocalDb.GetConnectionString(DbName), new User() { Name = "adamo" }, logger)
        {
        }
    }
}
