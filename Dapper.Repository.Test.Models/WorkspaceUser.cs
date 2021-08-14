using AO.Models;
using AO.Models.Attributes;
using BlazorAO.Models.Conventions;
using System.ComponentModel.DataAnnotations;

namespace BlazorAO.Models
{
    /// <summary>
    /// associates a user with a workspace, which is in effect an employee record
    /// </summary>
    [TrackChanges]
    public class WorkspaceUser : BaseTable
    {
        [Key]
        [References(typeof(Workspace))]
        public int WorkspaceId { get; set; }

        [Key]
        [References(typeof(UserProfile))]
        public int UserId { get; set; }

        [References(typeof(UserProfile))]
        public int? ManagerId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
