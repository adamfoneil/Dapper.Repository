using BlazorAO.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelSync.Models;
using SqlServer.LocalDb;
using SqlServer.LocalDb.Extensions;
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
    }
}
