using Dapper.QX.Abstract;
using Dapper.QX.Interfaces;
using System;
using System.Collections.Generic;

namespace Dapper.Repository.Test.Queries
{
    public class UserPermissionsResult
    {
        public string PermissionName { get; set; }
        public int Id { get; set; }
        public int WorkspaceId { get; set; }
        public int UserId { get; set; }
        public int PermissionId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? DateModified { get; set; }
    }

    public class UserPermissions : TestableQuery<UserPermissionsResult>
    {
        public UserPermissions() : base(
            @"SELECT
                [p].[Name] AS [PermissionName],
                [wup].*
            FROM
                [app].[Permission] [p]
                INNER JOIN [dbo].[WorkspaceUserPermission] [wup] ON [p].[Id]=[wup].[PermissionId]
                INNER JOIN [dbo].[AspNetUsers] [u] ON [wup].[UserId]=[u].[UserId]
            WHERE
                [u].[UserName]=@userName")
        {
        }

        public string UserName { get; set; }

        protected override IEnumerable<ITestableQuery> GetTestCasesInner()
        {
            yield return new UserPermissions() { UserName = "anyone" };
        }
    }
}
