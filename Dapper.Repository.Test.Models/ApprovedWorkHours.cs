using AO.Models;
using BlazorAO.Models.Conventions;
using System.ComponentModel.DataAnnotations;

namespace BlazorAO.Models
{
    /// <summary>
    /// indicates a manager's approval of a work record along with invoicing approval
    /// </summary>
    public class ApprovedWorkHours : BaseTable
    {
        [Key]
        [References(typeof(WorkHours))]
        public int WorkHoursId { get; set; }

        /// <summary>
        /// include on invoice? Based on Client.AllowInvoicing, but manager may override
        /// </summary>
        public bool AllowInvoicing { get; set; }
    }
}
