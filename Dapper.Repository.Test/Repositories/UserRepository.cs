using BlazorAO.Models;
using Dapper.Repository.SqlServer;
using System.Threading.Tasks;

namespace Dapper.Repository.Test.Repositories
{
    /// <summary>
    /// Some workarounds needed because dbo.AspNetUsers.Id is a string.
    /// Note that you can't do regular crud operations on this table
    /// </summary>
    public class UserRepository : Repository<User, UserProfile, string>
    {
        public UserRepository(SqlServerContext context) : base(context)
        {
        }
        
        /// <summary>
        /// to work around atypical behavior of Id in this table
        /// </summary>
        public async Task<UserProfile> GetByUserIdAsync(int id) => await GetWhereAsync(new { UserId = id });
    }
}
