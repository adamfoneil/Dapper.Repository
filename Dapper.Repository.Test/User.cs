using AO.Models.Interfaces;
using System;

namespace Dapper.Repository.Test
{
    public class User : IUserBase
    {
        public string Name { get; set; }
        public DateTime LocalTime => DateTime.UtcNow;
    }
}
