using AO.Models.Extensions;
using AO.Models.Interfaces;
using Dapper.QX.Abstract;
using Dapper.QX.Interfaces;
using System;
using System.Collections.Generic;

namespace Dapper.Repository.Test.Queries
{
    public class UserInfoResult : IUserBase
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public int UserId { get; set; }
        public string TimeZoneId { get; set; }
        public int? WorkspaceId { get; set; }
        public string WorkspaceName { get; set; }
        public DateTime LocalTime => Timestamp.Local(TimeZoneId);
        public IEnumerable<UserPermissionsResult> Permissions { get; set; } // gets set from Queries.UserPermissions

        public string Name => UserName;
    }

    public class UserInfo : TestableQuery<UserInfoResult>
    {
        public UserInfo() : base(
            @"SELECT 
                [u].[UserName],
                [u].[Email],
                [u].[UserId],
                [u].[TimeZoneId],
                [u].[WorkspaceId],
                [ws].[Name] AS [WorkspaceName]
            FROM
                [dbo].[AspNetUsers] [u]
                LEFT JOIN [dbo].[WorkspaceUser] [wu] ON [u].[UserId]=[wu].[UserId]
                LEFT JOIN [dbo].[Workspace] [ws] ON [wu].[WorkspaceId]=[ws].[Id]
            WHERE
                [u].[UserName]=@userName")
        {
        }

        public string UserName { get; set; }

        protected override IEnumerable<ITestableQuery> GetTestCasesInner()
        {
            yield return new UserInfo() { UserName = "anyone" };
        }
    }
}
