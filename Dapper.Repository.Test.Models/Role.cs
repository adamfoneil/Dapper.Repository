using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAO.Models
{
    /// <summary>
    /// defines roles administered by the app owner (distinct from a workspace or tenant owner)
    /// </summary>
    [Table("AspNetRoles")]        
    public class Role
    {
        [Key]
        [MaxLength(450)]
        [Column("Id")] // this is to work around atypical Id behavior
        public string RoleId { get; set; }

        [MaxLength(256)]
        public string Name { get; set; }

        [MaxLength(256)]        
        public string NormalizedName { get; set; }
     
        public string ConcurrencyStamp { get; set; }

        /// <summary>
        /// so that we can toggle in UI
        /// </summary>
        [NotMapped]
        public bool IsEnabled { get; set; }
    }
}
