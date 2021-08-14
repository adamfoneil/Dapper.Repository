using AO.Models;
using BlazorAO.Models.Conventions;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAO.Models
{
    public class RecurringTask : BaseTable
    {
        [Key]
        [References(typeof(Workspace))]
        public int WorkspaceId { get; set; }

        [Key]
        [References(typeof(RecurringTaskType))]
        public int TaskTypeId { get; set; }

        /// <summary>
        /// T-SQL DATEADD interval argument
        /// </summary>
        [MaxLength(10)]
        public string DateAddInterval { get; set; }

        /// <summary>
        /// T-SQL DATEADD number argument
        /// </summary>
        public int DateAddNumber { get; set; }

        [Column(TypeName = "date")]
        public DateTime NextDueDate { get; set; }

        [NotMapped]
        public string TaskName { get; set; }

        [NotMapped]
        public int DaysAway { get; set; }
    }
}
