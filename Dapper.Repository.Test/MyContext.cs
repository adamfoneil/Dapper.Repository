﻿using AO.Models.Enums;
using AO.Models.Interfaces;
using BlazorAO.Models;
using BlazorAO.Models.Conventions;
using Dapper.Repository.SqlServer;
using Microsoft.Extensions.Logging;
using SqlServer.LocalDb;
using System;
using System.Data;
using System.Threading.Tasks;

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

        public BaseRepository<Workspace> Workspaces => new BaseRepository<Workspace>(this);
        public BaseRepository<Client> Clients => new BaseRepository<Client>(this);
        public BaseRepository<Job> Jobs => new BaseRepository<Job>(this);
        public BaseRepository<Budget> Budgets => new BaseRepository<Budget>(this);
    }

    public class BaseRepository<TModel> : Repository<TModel, int> where TModel : IModel<int>
    {
        public BaseRepository(SqlServerContext context) : base(context)
        {
        }

        protected new async Task BeforeSaveAsync(IDbConnection connection, SaveAction action, TModel model, IDbTransaction txn = null)
        {
            if (model is BaseTable baseTable)
            {
                switch (action)
                {
                    case SaveAction.Insert:
                        baseTable.CreatedBy = Context.User.Name;
                        baseTable.DateCreated = Context.User.LocalTime;
                        break;

                    case SaveAction.Update:
                        baseTable.ModifiedBy = Context.User.Name;
                        baseTable.DateModified = Context.User.LocalTime;
                        break;
                }
            }

            await Task.CompletedTask;
        }
    }
}
