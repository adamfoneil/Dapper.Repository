using AO.Models;
using AO.Models.Attributes;
using BlazorAO.Models.Conventions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAO.Models
{
    [TrackChanges]
    public class InvoiceRate : BaseTable
    {
        [Key]
        [References(typeof(WorkType))]        
        public int WorkTypeId { get; set; }

        /// <summary>
        /// use 0 for all employees or a specific WorkspaceUser.Id
        /// </summary>
        [Key]
        public int WorkspaceUserId { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }
    }
}
