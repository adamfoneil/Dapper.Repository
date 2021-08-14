using AO.Models;
using AO.Models.Attributes;
using BlazorAO.Models.Conventions;
using System.ComponentModel.DataAnnotations;

namespace BlazorAO.Models
{
    [TrackChanges]
    public class Client : BaseTable
    {
        [Key]
        [References(typeof(Workspace))]
        public int WorkspaceId { get; set; }

        [Key]
        [MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// if true, then we can invoice this client for work done
        /// if false, then it's considered overhead or an internal client
        /// </summary>
        public bool AllowInvoicing { get; set; }

        [MaxLength(50)]
        public string InvoiceEmail { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
