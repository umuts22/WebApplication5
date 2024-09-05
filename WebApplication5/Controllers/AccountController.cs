using Microsoft.AspNetCore.Mvc;
using WebApplication5.Models;
using WebApplication5.DAL;
using Microsoft.EntityFrameworkCore;
using WebApplication5.ViewModels;

namespace WebApplication5.Controllers
{
    public class AccountController : Controller
    {
        private readonly BankingDbContext _context;

        public AccountController(BankingDbContext context)
        {
            _context = context;
        }

        // GET: Account/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .FromSqlInterpolated($"EXEC dbo.GetCustomerAccounts @AccountID = {id}")
                .Include(a => a.Customer) 
                .FirstOrDefaultAsync();

            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // GET: Account/Update/5
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .FromSqlInterpolated($"EXEC dbo.GetCustomerAccounts @AccountID = {id}")
                .FirstOrDefaultAsync();

            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }

        // POST: Account/Update/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [Bind("AccountID,CustomerID,AccountNumber,Balance")] Account account)
        {
            if (id != account.AccountID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.Database.ExecuteSqlInterpolatedAsync(
                        $"EXEC dbo.UpdateAccount @AccountID = {account.AccountID}, @CustomerID = {account.CustomerID}, @AccountNumber = {account.AccountNumber}, @Balance = {account.Balance}");

                    return RedirectToAction(nameof(Details), new { id = account.AccountID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await AccountExists(account.AccountID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(account);
        }

        private async Task<bool> AccountExists(int id)
        {
            var account = await _context.Accounts
                .FromSqlInterpolated($"EXEC dbo.GetCustomerAccounts @AccountID = {id}")
                .FirstOrDefaultAsync();

            return account != null;
        }


        // GET: Account/Index
        public async Task<IActionResult> Index()
        {

            var accounts = await _context.Accounts.ToListAsync();

            var viewModel = new AccountsViewModel
            {
                Accounts = accounts
            };

            return View(viewModel);
        }

    }
}
