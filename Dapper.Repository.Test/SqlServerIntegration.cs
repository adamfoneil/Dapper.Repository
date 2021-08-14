using AO.Models.Static;
using BlazorAO.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Models;
using SqlServer.LocalDb;
using SqlServer.LocalDb.Extensions;
using System;
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
            using (var cn = LocalDb.GetConnection(MyContext.DbName))
            {
                var sample = new Client(); // jiggles the reference collection
                var assembly = Assembly.GetExecutingAssembly().GetReferencedAssembly("Dapper.Repository.Test.Models");
                var types = assembly.GetExportedTypes().Where(t => t.Namespace.Equals("BlazorAO.Models")).ToArray();
                await cn.DropAllTablesAsync();
                DataModel.CreateTablesAsync(types, cn).Wait();
            }            
        }

        private MyContext GetContext()
        {
            var logger = LoggerFactory.Create(config => config.AddDebug()).CreateLogger("Testing");
            return new MyContext(logger);
        }

        [TestMethod]
        public async Task SaveAndDeleteWorkspace()
        {
            var context = GetContext();

            var ws = await context.Workspaces.SaveAsync(new Workspace()
            {
                Name = "sample workspace"
            });

            ws.StorageContainer = "whatever";
            await context.Workspaces.SaveAsync(ws);

            ws = await context.Workspaces.GetAsync(ws.Id);
            Assert.IsTrue(ws.StorageContainer.Equals("whatever"));

            await context.Workspaces.DeleteAsync(ws);
        }

        [TestMethod]
        public async Task CustomIdentity()
        {
            var context = GetContext();

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
    }
}
