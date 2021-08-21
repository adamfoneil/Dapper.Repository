using BlazorAO.Models;
using Dapper.Repository.SqlServer.Extensions;
using Dapper.Repository.Test.Contexts;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.LocalDb;
using System;
using System.Threading.Tasks;

namespace Dapper.Repository.Test.Tests
{
    [TestClass]
    public class ExtensionMethods
    {
        [ClassInitialize]
        public static async Task Initialize(TestContext context) => await SqlServerIntegration.BuildLocalDatabase();

        [TestMethod]
        public async Task Insert()
        {
            using var cn = LocalDb.GetConnection(SimpleContext.DbName);
            Workspace ws = await InsertWorkspace(cn, "whatever workspace");

            Assert.IsTrue(ws.Id != 0);
        }

        private static async Task<Workspace> InsertWorkspace(SqlConnection cn, string name)
        {
            return await cn.InsertAsync<Workspace, int>(new Workspace()
            {
                Name = name,
                StorageContainer = "yayah",
                StorageConnectionString = "something that obviously doesn't work",
                DateCreated = DateTime.Now,
                CreatedBy = "nobody"
            }, afterInsert: (model, id) =>
            {
                model.Id = id;
            });
        }

        [TestMethod]
        public async Task Update()
        {
            using var cn = LocalDb.GetConnection(SimpleContext.DbName);

            var ws = await InsertWorkspace(cn, "yahooie");
            var id = ws.Id;

            ws.Name = "new name";
            ws.StorageContainer = "a new container";

            await cn.UpdateAsync(ws);

            ws = await cn.GetAsync<Workspace, int>(id);
            Assert.IsTrue(ws.Name.Equals("new name"));
            Assert.IsTrue(ws.StorageContainer.Equals("a new container"));
        }

        [TestMethod]
        public async Task Delete()
        {
            using var cn = LocalDb.GetConnection(SimpleContext.DbName);
            var ws = await InsertWorkspace(cn, "zithra");
            var id = ws.Id;

            await cn.DeleteAsync<Workspace, int>(id);

            // doesn't exist after deletion
            var get = await cn.GetAsync<Workspace, int>(id);
            Assert.IsNull(get);
        }
    }
}
