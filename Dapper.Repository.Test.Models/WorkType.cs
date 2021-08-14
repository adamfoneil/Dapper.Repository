using AO.Models;
using AO.Models.Attributes;
using BlazorAO.Models.Conventions;
using System.ComponentModel.DataAnnotations;

namespace BlazorAO.Models
{
    /// <summary>
    /// a specific type of work or service provided with a certain hourly rate
    /// </summary>
    [TrackChanges]
    public class WorkType : BaseTable
    {
        [Key]
        [References(typeof(Workspace))]
        public int WorkspaceId { get; set; }

        [Key]
        [MaxLength(50)]
        public string Name { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
