using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication5.DAL;
using WebApplication5.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication5.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly BankingDbContext _context;

        public DashboardController(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
           
            int customerId = 2;



            var customer = await _context.Customers
                .Where(c => c.CustomerID == customerId)
                .Select(c => new
                {
                    FullName = c.FirstName + " " + c.LastName // Combine first and last name
                })
                .FirstOrDefaultAsync();

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

            var debitCreditData = new DebitCreditChartDataViewModel
            {
                Labels = new List<string>(), // Add the actual labels here
                DebitData = new List<decimal>(),
                CreditData = new List<decimal>()
            };

            var transactionCommandText = "EXEC dbo.GetDebitCreditTransactions @CustomerID";
            using (var transactionCommand = _context.Database.GetDbConnection().CreateCommand())
            {
                transactionCommand.CommandText = transactionCommandText;
                transactionCommand.Parameters.Add(new SqlParameter("@CustomerID", customerId));
                _context.Database.OpenConnection();
                using (var reader = await transactionCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        debitCreditData.Labels.Add(reader.GetDateTime(reader.GetOrdinal("Date")).ToString("dd MMM"));

                        // Check the transaction type and add the amount to the appropriate list
                        var transactionType = reader.GetString(reader.GetOrdinal("Type"));
                        var amount = reader.GetDecimal(reader.GetOrdinal("Amount"));

                        if (transactionType == "debit")
                        {
                            debitCreditData.DebitData.Add(amount);
                            debitCreditData.CreditData.Add(0);  // Add 0 for credit when it's a debit transaction
                        }
                        else if (transactionType == "credit")
                        {
                            debitCreditData.DebitData.Add(0);  // Add 0 for debit when it's a credit transaction
                            debitCreditData.CreditData.Add(amount);
                        }
                    }
                }

              
            }


            // Fetch transaction history using raw SQL query
            var transactions = await _context.Transactions
                .FromSqlInterpolated($"EXEC dbo.GetTransactionHistoryCid @Customerid = {customerId}")
                .ToListAsync();

            var model = new DashboardViewModel
            {
                TotalBalance = totalBalance,
                TotalCredit = totalCredit,
                TotalSaving = totalBalance - totalCredit,  // Example calculation
                CustomerName = customer.FullName,  // Set the full name here
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
        },
                DebitCreditChartData = debitCreditData
            };

            Console.WriteLine("Labels: " + string.Join(", ", debitCreditData.Labels));
            Console.WriteLine("Debit Data: " + string.Join(", ", debitCreditData.DebitData));
            Console.WriteLine("Credit Data: " + string.Join(", ", debitCreditData.CreditData));
            return View("~/Views/Dashboard/Index.cshtml", model);
        }

    }
}
