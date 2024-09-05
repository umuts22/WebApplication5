using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication5.DAL;
using WebApplication5.ViewModels;
using Microsoft.Data.SqlClient;

namespace WebApplication5.Controllers
{
    public class DashboardController : Controller
    {
        private readonly BankingDbContext _context;

        public DashboardController(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Example customer ID, replace with actual logic to get current customer's ID
            int customerId = 2;

            // Fetch the customer's name
            var customer = await _context.Customers
                .Where(c => c.CustomerID == customerId)
                .Select(c => c.CustomerName) // Assuming your Customers table has a CustomerName field
                .FirstOrDefaultAsync();


            // Fetch total balance using the new stored procedure
            decimal totalBalance;
            var commandText = "EXEC GetTotalBalance @CustomerID";
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = commandText;
                command.Parameters.Add(new SqlParameter("@CustomerID", customerId));
                _context.Database.OpenConnection();
                totalBalance = (decimal)await command.ExecuteScalarAsync();
            }

            // Example value for total credit
            var totalCredit = 1000m;

            // Fetch transaction history using raw SQL query
            var transactions = await _context.Transactions
                .FromSqlInterpolated($"EXEC dbo.GetTransactionHistory @AccountID = {customerId}")
                .ToListAsync();

            var model = new DashboardViewModel
            {
                TotalBalance = totalBalance,
                TotalCredit = totalCredit,
                Transactions = transactions.Select(t => new TransactionViewModel
                {
                    Description = t.Description,
                    Category = t.TransactionType,
                    Date = t.TransactionDate.ToString("dd MMM yyyy"),
                    Status = t.TransactionType, // Assuming status is represented by the type
                    Amount = t.Amount
                }).ToList(),
                Invoices = new List<InvoiceViewModel>
                {
                    new InvoiceViewModel { ClientName = "Apple Store", TimeAgo = "5h ago", Amount = 450 },
                    new InvoiceViewModel { ClientName = "Michael", TimeAgo = "2 days ago", Amount = 160 },
                    new InvoiceViewModel { ClientName = "Playstation", TimeAgo = "5 days ago", Amount = 1085 },
                    new InvoiceViewModel { ClientName = "William", TimeAgo = "10 days ago", Amount = 90 }
                }
            };

            return View("~/Views/Dashboard/Index.cshtml", model);
        }
    }
}
