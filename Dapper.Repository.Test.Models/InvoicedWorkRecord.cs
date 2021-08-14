using AO.Models;
using BlazorAO.Models.Conventions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAO.Models
{
    /// <summary>
    /// links a work record to an invoice at a project's current invoice rate
    /// </summary>
    public class InvoicedWorkRecord : BaseTable
    {
        [Key]
        [References(typeof(WorkHours))]
        public int WorkHoursId { get; set; }

        [References(typeof(Invoice))]
        public int InvoiceId { get; set; }

        /// <summary>
        /// invoice rate (based on InvoiceRate.Amount)
        /// </summary>
        [Column(TypeName = "money")]
        public decimal Rate { get; set; }
    }
}
