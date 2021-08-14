using AO.Models;
using BlazorAO.Models.Conventions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorAO.Models
{
    public class Job : BaseTable
    {
        [Key]
        [References(typeof(Client))]
        public int ClientId { get; set; }

        [Key]
        [MaxLength(50)]
        public string Name { get; set; }

        [References(typeof(UserProfile))]
        public int? ManagerId { get; set; }

        public bool IsActive { get; set; } = true;

        [NotMapped]
        public string ClientName { get; set; }

        [NotMapped]
        public string ManagerName { get; set; }

        [NotMapped]
        public string JobDisplayName { get; set; }

        [NotMapped]
        public decimal? CurrentBudget { get; set; }

        [NotMapped]
        public decimal? TotalBudget { get; set; }

        [NotMapped]
        public decimal? TotalInvoices { get; set; }
    }
}
