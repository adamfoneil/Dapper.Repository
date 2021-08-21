using AO.Models.Static;
using BlazorAO.Models;
using Dapper.Repository.SqlServer.Extensions;
using Dapper.Repository.Test.Contexts;
using Dapper.Repository.Test.Queries;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Models;
using SqlServer.LocalDb;
using SqlServer.LocalDb.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dapper.Repository.Test
{
    [TestClass]
    public class SqlServerIntegration
    {
        [ClassInitialize]
        public static async Task Initialize(TestContext context)
        {
            using (var cn = LocalDb.GetConnection(SimpleContext.DbName))
            {
                var sample = new Client(); // jiggles the reference collection
                var assembly = Assembly.GetExecutingAssembly().GetReferencedAssembly("Dapper.Repository.Test.Models");
                var types = assembly.GetExportedTypes().Where(t => t.Namespace.Equals("BlazorAO.Models")).ToArray();
                await cn.DropAllTablesAsync();
                DataModel.CreateTablesAsync(types, cn).Wait();

                // help from https://www.learmoreseekmore.com/2020/04/net-core-distributed-sql-server-cache.html
                await cn.ExecuteAsync(
                    @"CREATE TABLE [dbo].[Cache] (
                        [Id] NVARCHAR(900) NOT NULL PRIMARY KEY, 
                        [Value] VARBINARY(MAX) NOT NULL, 
                        [ExpiresAtTime] DATETIMEOFFSET NOT NULL, 
                        [SlidingExpirationInSeconds] BIGINT NULL, 
                        [AbsoluteExpiration] DATETIMEOFFSET NULL
                    )");

                await cn.InsertAsync(new UserProfile()
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "adamo",
                    Email = "adamosoftware@gmail.com",
                    TimeZoneId = "Eastern Standard Time",
                    FirstName = "Adam",
                    LastName = "O'Neil"
                }, new[]
                {
                    "Id", "UserName", "Email", "TimeZoneId", "FirstName", "LastName", 
                    "EmailConfirmed", "PhoneNumberConfirmed", "TwoFactorEnabled"
                });
            }            
        }

        private const string UserName = "adamo";

        private SimpleContext GetSimpleContext()
        {
            var logger = LoggerFactory.Create(config => config.AddDebug()).CreateLogger("Testing");
            return new SimpleContext(UserName, logger);
        }

        private RealisticContext GetRealisticContext()
        {
            var logger = LoggerFactory.Create(config => config.AddDebug()).CreateLogger("Testing");
            
            return new RealisticContext(UserName, GetCache(), logger);
        }

        private IDistributedCache GetCache()
        {
            var options = new SqlServerCacheOptions()
            {
                ConnectionString = LocalDb.GetConnectionString(SimpleContext.DbName),
                SchemaName = "dbo",
                TableName = "Cache"
            };
            return new SqlServerCache(options);
        }

        [TestMethod]
        public async Task SaveAndDeleteWorkspaceSimple() => await SaveAndDeleteWorkspace(GetSimpleContext());

        [TestMethod]
        public async Task SaveAndDeleteWorkspaceRealistic() => await SaveAndDeleteWorkspace(GetRealisticContext());

        [TestMethod]
        public async Task CanUseCache()
        {
            var context = GetRealisticContext();

            // store user in cache table (wouldn't do in a real app)
            await context.CacheUserAsync();

            // force user to be queried again, but user should come from cache (which is a table for test purposes)
            context.Logout();

            // do some normal crud operations, which will cause the user to be queried
            await SaveAndDeleteWorkspace(context);            

            // and the user should come from the cache source
            Assert.IsTrue(context.ProfileSource == ProfileSourceOptions.Cache);
        }

        [TestMethod]
        public async Task CustomIdentity()
        {
            var context = GetSimpleContext();

            var insert = SqlBuilder.Insert<UserProfile>(new string[]
            {
                "Id", "UserName", "Email", "EmailConfirmed", "PhoneNumberConfirmed", "TwoFactorEnabled"
            }) + " SELECT SCOPE_IDENTITY()";

            const string email = "hello@nowhere.org";

            using var cn = context.GetConnection();
            var id = await cn.QuerySingleOrDefaultAsync<int>(insert, new UserProfile()
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "hello",
                Email = email,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false
            });

            var user = await context.Users.GetByUserIdAsync(id);

            Assert.IsTrue(user.Email.Equals(email));
        }

        [TestMethod]
        public async Task WithValidation()
        {
            var context = GetSimpleContext();

            try
            {
                var hours = await context.WorkHours.SaveAsync(new WorkHours()
                {
                    Hours = -1
                });

                Assert.Fail("Should not be able to save this row");
            }
            catch (Exception exc)
            {
                Assert.IsTrue(exc is ValidationException && exc.Message.Equals("Hours must be greater than zero."));
            }
        }

        private async Task SaveAndDeleteWorkspace(IAppContext context)
        {
            var ws = await context.Workspaces.SaveAsync(new Workspace()
            {
                Name = "sample workspace"
            });

            Assert.IsTrue(ws.CreatedBy.Equals(UserName));

            ws.StorageContainer = "whatever";
            await context.Workspaces.SaveAsync(ws);

            ws = await context.Workspaces.GetAsync(ws.Id);
            Assert.IsTrue(ws.StorageContainer.Equals("whatever"));            

            Assert.IsTrue(ws.ModifiedBy.Equals(UserName));

            await context.Workspaces.DeleteAsync(ws);
        }
    }
}
