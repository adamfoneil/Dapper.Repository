﻿using BlazorAO.Models;
using Dapper.Repository.SqlServer.Extensions;
using Dapper.Repository.Test.Contexts;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.LocalDb;
using System;
using System.Collections.Generic;
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
            using var cn = LocalDb.GetConnection(DataContext.DbName);
            Workspace ws = await InsertWorkspace(cn, "whatever workspace");

            Assert.IsTrue(ws.Id != 0);
        }

        [TestMethod]
        public async Task InsertWithDictionary()
        {
            using var cn = LocalDb.GetConnection(DataContext.DbName);
            var id = await cn.InsertAsync("dbo.Workspace", new Dictionary<string, object>()
            {
                ["Name"] = "hello workspace",
                ["NextInvoice"] = 1020,
                ["StorageContainer"] = "this container",
                ["CreatedBy"] = "test",
                ["DateCreated"] = DateTime.Now
            });            

            var ws = await cn.GetAsync<Workspace, int>(Convert.ToInt32(id));

            Assert.IsTrue(ws.Name.Equals("hello workspace"));
            Assert.IsTrue(ws.NextInvoice.Equals(1020));
            Assert.IsTrue(ws.StorageContainer.Equals("this container"));
        }

        [TestMethod]
        public async Task UpdateWithDictionary()
        {
            using var cn = LocalDb.GetConnection(DataContext.DbName);
            var ws = await InsertWorkspace(cn, "an udpate test");

            await cn.UpdateAsync("dbo.Workspace", new Dictionary<string, object>()
            {
                ["Name"] = "hello workspace1",
                ["NextInvoice"] = 1020,
                ["StorageContainer"] = "this container",
                ["Id"] = ws.Id
            });

            ws = await cn.GetAsync<Workspace, int>(ws.Id);

            Assert.IsTrue(ws.Name.Equals("hello workspace1"));
            Assert.IsTrue(ws.NextInvoice.Equals(1020));
            Assert.IsTrue(ws.StorageContainer.Equals("this container"));
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
            using var cn = LocalDb.GetConnection(DataContext.DbName);

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
            using var cn = LocalDb.GetConnection(DataContext.DbName);
            var ws = await InsertWorkspace(cn, "zithra");
            var id = ws.Id;

            await cn.DeleteAsync<Workspace, int>(id);

            // doesn't exist after deletion
            var get = await cn.GetAsync<Workspace, int>(id);
            Assert.IsNull(get);
        }

        [TestMethod]
        public async Task GetWhere()
        {
            using var cn = LocalDb.GetConnection(DataContext.DbName);
            await DeleteWhatevers(cn);

            Workspace ws = await InsertWorkspace(cn, "whatever workspace2");

            ws = await cn.GetWhereAsync<Workspace>(new { name = "whatever workspace2" });
            Assert.IsTrue(ws != null);
        }

        [TestMethod]
        public async Task ExistsWhere()
        {
            using var cn = LocalDb.GetConnection(DataContext.DbName);
            await DeleteWhatevers(cn);

            Workspace ws = await InsertWorkspace(cn, "whatever workspace3");
            var exists = await cn.ExistsWhereAsync<Workspace>(new { name = "whatever workspace3" });
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public async Task Merge()
        {
            using var cn = LocalDb.GetConnection(DataContext.DbName);
            await DeleteWhatevers(cn);

            const string WsName = "whatever workspace3";

            // should insert
            var ws = await cn.MergeAsync<Workspace, int>(new Workspace()
            {
                Name = WsName,
                CreatedBy = "adamo",
                DateCreated = DateTime.Now
            });

            // should update
            ws = await cn.MergeAsync<Workspace, int>(new Workspace()
            {
                Name = WsName,
                StorageContainer = "hello"
            });

            ws = await cn.GetWhereAsync<Workspace>(new { name = WsName });
            Assert.IsTrue(ws.StorageContainer.Equals("hello"));
        }

        private static async Task DeleteWhatevers(SqlConnection cn)
        {
            await cn.ExecuteAsync("DELETE [dbo].[Workspace] WHERE [Name] LIKE 'whatever%'");
        }
    }
}
