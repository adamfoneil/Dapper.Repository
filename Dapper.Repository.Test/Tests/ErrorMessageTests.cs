using AO.Models.Interfaces;
using Dapper.Repository.Exceptions;
using Dapper.Repository.Interfaces;
using Dapper.Repository.MessageHandlers;
using Dapper.Repository.SqlServer;
using Dapper.Repository.SqlServer.MessageHandlers;
using Dapper.Repository.Test.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.LocalDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Dapper.Repository.Test.Tests.SampleContext;

namespace Dapper.Repository.Test.Tests
{
    [TestClass]
    public class ErrorMessageTests
    {
        [TestMethod]
        public async Task InsertPKError()
        {
            using var cn = LocalDb.GetConnection(DbName);
            await CreateObjectsAsync(cn);

            var logger = LoggerFactory
                .Create(config => config.AddConsole())
                .CreateLogger<SampleContext>();

            var ctx = new SampleContext(logger);

            await ctx.Categories.SaveAsync(new Category()
            {
                Name = "hello"
            });

            try
            {
                await ctx.Categories.SaveAsync(new Category()
                {
                    Name = "hello"
                });
            }
            catch (Exception exc)
            {
                Assert.IsTrue(exc.Message.Equals("Can't save this row because the value 'hello' is already in use in table 'dbo.Category'."));

                var repoExc = exc as RepositoryException;
                Assert.IsNotNull(repoExc);
                Assert.IsTrue(repoExc.Sql.IsEqualWithoutWhitespace(
                    @"INSERT INTO [Category] (
                        [Name]
                    ) VALUES (
                        @Name
                    ); SELECT SCOPE_IDENTITY();"));
            }
        }

        [TestMethod]
        public async Task DeleteFKError()
        {
            using var cn = LocalDb.GetConnection(DbName);
            await CreateObjectsAsync(cn);

            var logger = LoggerFactory
                .Create(config => config.AddConsole())
                .CreateLogger<SampleContext>();

            var ctx = new SampleContext(logger);

            await ctx.Categories.SaveAsync(new Category() { Name = "Whatever" });
            await ctx.Items.SaveAsync(new Item() { CategoryId = 1, Name = "this" });
            await ctx.Items.SaveAsync(new Item() { CategoryId = 1, Name = "that" });

            try
            {
                await ctx.Categories.DeleteAsync(new Category() { Id = 1 });
            }
            catch (Exception exc)
            {
                Assert.IsTrue(exc.Message.Equals("Can't delete 'Category' row because at least one 'Item' row is depending on it."));

                var repoExc = exc as RepositoryException;
                Assert.IsNotNull(repoExc);
            }
        }

        [TestMethod]
        public async Task InsertFKError()
        {
            using var cn = LocalDb.GetConnection(DbName);
            await CreateObjectsAsync(cn);

            var logger = LoggerFactory
                .Create(config => config.AddConsole())
                .CreateLogger<SampleContext>();

            var ctx = new SampleContext(logger);

            await ctx.Categories.SaveAsync(new Category() { Name = "Whatever" });                        

            try
            {
                await ctx.Items.SaveAsync(new Item() { CategoryId = 0, Name = "this" });
            }
            catch (Exception exc)
            {
                Assert.IsTrue(exc.Message.Equals("Can't save the 'Item' row because of a missing or unrecognized value in the 'CategoryId' field(s)."));

                var repoExc = exc as RepositoryException;
                Assert.IsNotNull(repoExc);
            }
        }

        private async Task CreateObjectsAsync(SqlConnection cn)
        {
            await cn.ExecuteAsync(
                @"DROP TABLE IF EXISTS [dbo].[Item]; 

                DROP TABLE IF EXISTS [dbo].[Category];
                CREATE TABLE [dbo].[Category] (
                    [Id] int identity(1,1),
                    [Name] nvarchar(50) NOT NULL PRIMARY KEY,
                    CONSTRAINT [U_Category_Id] UNIQUE ([Id])
                );

                CREATE TABLE [dbo].[Item] (
                    [Id] int identity(1,1),
                    [CategoryId] int NOT NULL,
                    [Name] nvarchar(50) NOT NULL,
                    CONSTRAINT [PK_Item] PRIMARY KEY ([CategoryId], [Name]),
                    CONSTRAINT [U_Item_Id] UNIQUE ([Id]),
                    CONSTRAINT [FK_Item_Category] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Category] ([Id])
                );");
        }
    }

    public class SampleContext : SqlServerContext<SampleUser>
    {
        public const string DbName = "SampleContext";

        private static IEnumerable<IErrorMessageHandler> DefaultHandlers => new IErrorMessageHandler[]
        {
            new DeleteCascadeBlocked((info) => $"Can't delete '{info.ReferencedTable}' row because at least one '{info.ReferencingTable}' row is depending on it."),
            new InvalidForeignKeyValue((info) => $"Can't save the '{info.ReferencingTable}' row because of a missing or unrecognized value in the '{string.Join(", ", info.Columns.Select(col => col.ReferencingName))}' field(s)."),
            new DuplicateKeyError((value, tableName) => $"Can't save this row because the value '{value}' is already in use in table '{tableName}'.")
        };

        public SampleContext(ILogger<SampleContext> logger) : base(LocalDb.GetConnectionString(DbName), logger, DefaultHandlers)
        {
        }

        public BaseRepository<Category> Categories => new BaseRepository<Category>(this);
        public BaseRepository<Item> Items => new BaseRepository<Item>(this);

        public class SampleUser
        {
            public string Name { get; set; }
        }
    }

    public class BaseRepository<TModel> : Repository<SampleUser, TModel, int> where TModel : IModel<int>
    {
        public BaseRepository(SampleContext context) : base(context)
        {
        }
    }

    public class Category : IModel<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Item : IModel<int>
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }
}
