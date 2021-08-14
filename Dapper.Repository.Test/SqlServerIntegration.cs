using BlazorAO.Models;
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

        [TestMethod]
        public void TestMethod1()
        {
        }
    }    

}
