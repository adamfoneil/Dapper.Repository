using BlazorAO.Models;
using Dapper.Repository.Test.Contexts;

namespace Dapper.Repository.Test.Repositories
{
    public class JobRepository : BaseRepository<Job>
    {
        public JobRepository(DataContext context) : base(context)
        {
        }

        protected override string SqlGet => query;
        
        private const string CurrentBudgetQuery = "(SELECT SUM([Amount]) FROM [dbo].[Budget] [b] WHERE [JobId]=[j].[Id] AND DATEFROMPARTS([b].[Year], [b].[Month], 1) <= DATEFROMPARTS(YEAR(getdate()), MONTH(getdate()), 1))";
        private const string TotalBudgetQuery = "(SELECT SUM([Amount]) FROM [dbo].[Budget] [b] WHERE [JobId]=[j].[Id])";
        private const string TotalInvoicesQuery = "(SELECT SUM([Amount]) FROM [dbo].[Invoice] WHERE [JobId]=[j].[Id])";

        private string query =
            $@"SELECT 
                [j].*,
                [c].[Name] AS [ClientName],
                [u].[LastName] + ', ' + [u].[FirstName] AS [ManagerName],
                [c].[Name] + ' - ' + [j].[Name] AS [JobDisplayName],
                {CurrentBudgetQuery} AS [CurrentBudget],
                {TotalBudgetQuery} AS [TotalBudget],
                {TotalInvoicesQuery} AS [TotalInvoices]
            FROM 
                [dbo].[Job] [j]
                INNER JOIN [dbo].[Client] [c] ON [j].[ClientId]=[c].[Id]
                LEFT JOIN [dbo].[AspNetUsers] [u] ON [j].[ManagerId]=[u].[UserId]
            WHERE 
                [j].[Id]=@id";
    }
}
