using AO.Models;
using BlazorAO.Models.Conventions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAO.Models
{
    /// <summary>
    /// amount approved by a client for invoicing during a month
    /// </summary>
    public class Budget : BaseTable
    {
        [Key]
        [References(typeof(Job))]
        public int JobId { get; set; }

        [Key]
        public int Year { get; set; }

        [Key]
        public int Month { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }
    }
}
