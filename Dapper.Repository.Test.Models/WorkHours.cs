using AO.Models;
using BlazorAO.Models.Conventions;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAO.Models
{
    /// <summary>
    /// record of hours worked by an employee on a job
    /// </summary>
    public class WorkHours : BaseTable
    {
        [References(typeof(UserProfile))]
        public int UserId { get; set; }

        [References(typeof(Workspace))]
        public int WorkspaceId { get; set; }

        [References(typeof(Job))]
        public int JobId { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [References(typeof(WorkType))]
        public int WorkTypeId { get; set; }

        /// <summary>
        /// number of hours worked
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal Hours { get; set; }

        [MaxLength(255)]
        public string Comments { get; set; }

        /// <summary>
        /// submitted for approval
        /// </summary>
        public bool IsSubmitted { get; set; }
    }
}
