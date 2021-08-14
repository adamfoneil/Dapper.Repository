using AO.Models.Interfaces;
using AO.Models.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BlazorAO.Models
{
    public class UserProfile : UserProfileBase, ITenantUser<int>, IUserBaseWithRoles
    {
        public int? WorkspaceId { get; set; }        

        public int TenantId => WorkspaceId ?? 0;

        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]        
        public string LastName { get; set; }

        public HashSet<string> Roles { get; set; }

        public bool HasRole(string roleName) => Roles?.Contains(roleName) ?? false;

        public bool HasAnyRole(params string[] roleNames) => roleNames?.Any(role => HasRole(role)) ?? false;
    }
}
