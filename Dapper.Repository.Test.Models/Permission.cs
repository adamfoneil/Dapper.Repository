using AO.Models;
using BlazorAO.Models.Conventions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorAO.Models
{
    /// <summary>
    /// defines permissions at the workspace level (tenant-specific) as opposed to AspNetRole (application-level)
    /// </summary>
    public class Permission : AppTable
    {
        [Key]
        [MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// what role do you need to grant this one?
        /// </summary>
        [References(typeof(Permission))]
        public int? GrantedByPermissionId { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        public static IEnumerable<Permission> SeedData => new Permission[]
        {
            new Permission() { Name = "President", GrantedByPermissionId = 1, Description = "May manage users, delete invoices" },
            new Permission() { Name = "Project Manager", GrantedByPermissionId = 1, Description = "May create projects and set budgets, approve expenses" },
            new Permission() { Name = "Accountant", GrantedByPermissionId = 1, Description = "May create and invoice clients" },
            new Permission() { Name = "Supervisor", GrantedByPermissionId = 2, Description = "May approve employee hours" }            
        };
    }   
}
