using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication5.Models;

namespace WebApplication5.DAL
{
    public class BankingDbContext : DbContext
    {
        public BankingDbContext(DbContextOptions<BankingDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        // Create a customer
        public async Task CreateCustomerAsync(string firstName, string lastName, string email, string phoneNumber)
        {
            await Database.ExecuteSqlRawAsync(
                "EXEC dbo.CreateCustomer @p0, @p1, @p2, @p3",
                firstName, lastName, email, phoneNumber);
        }

        // Create an account
        public async Task CreateAccountAsync(int customerId, string accountNumber, decimal balance)
        {
            await Database.ExecuteSqlRawAsync(
                "EXEC dbo.CreateAccount @p0, @p1, @p2",
                customerId, accountNumber, balance);
        }

        // Deposit funds
        public async Task DepositFundsAsync(int accountId, decimal amount, string description, string createdBy)
        {
            await Database.ExecuteSqlRawAsync(
                "EXEC dbo.DepositFunds @p0, @p1, @p2, @p3",
                accountId, amount, description, createdBy);
        }

        // Get account balance
        public async Task<decimal> GetAccountBalanceAsync(int accountId)
        {
            var balance = await Accounts
                .FromSqlRaw("EXEC dbo.GetAccountBalance @p0", accountId)
                .AsNoTracking()
                .SingleOrDefaultAsync();

            return balance?.Balance ?? 0;
        }

        // Get transaction history
        public async Task<IEnumerable<Transaction>> GetTransactionHistoryAsync(int accountId)
        {
            return await Transactions
                .FromSqlRaw("EXEC dbo.GetTransactionHistory @p0", accountId)
                .AsNoTracking()
                .ToListAsync();
        }

        // Withdraw funds
        public async Task WithdrawFundsAsync(int accountId, decimal amount, string description, string createdBy)
        {
            await Database.ExecuteSqlRawAsync(
                "EXEC dbo.WithdrawFunds @p0, @p1, @p2, @p3",
                accountId, amount, description, createdBy);
        }

        public async Task TransferFundsAsync(int fromAccountId, int toAccountId, decimal amount)
{
    await Database.ExecuteSqlRawAsync(
        "EXEC dbo.TransferFunds @p0, @p1, @p2",
        fromAccountId, toAccountId, amount);
}

    }
}
