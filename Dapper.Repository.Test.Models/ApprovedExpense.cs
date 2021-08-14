using AO.Models;
using BlazorAO.Models.Conventions;
using System.ComponentModel.DataAnnotations;

namespace BlazorAO.Models
{
    /// <summary>
    /// represents project manager's approval of an expense (typically an employee reimbursement)
    /// </summary>
    public class ApprovedExpense : BaseTable
    {
        [Key]
        [References(typeof(Expense))]
        public int ExpenseId { get; set; }
    }
}
