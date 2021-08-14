using AO.Models;
using BlazorAO.Models.Conventions;
using System.ComponentModel.DataAnnotations;

namespace BlazorAO.Models
{
    /// <summary>
    /// assigns a role to a user in a workspace
    /// </summary>
    public class WorkspaceUserPermission : BaseTable
    {
        [Key]
        [References(typeof(Workspace))]        
        public int WorkspaceId { get; set; }

        [Key]
        [References(typeof(UserProfile))]
        public int UserId { get; set; }

        [Key]
        [References(typeof(Permission))]
        public int PermissionId { get; set; }
    }
}
