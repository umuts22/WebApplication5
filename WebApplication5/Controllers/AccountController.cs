using Microsoft.AspNetCore.Mvc;
using WebApplication5.Models;
using WebApplication5.DAL;
using Microsoft.EntityFrameworkCore;
using WebApplication5.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var accounts = await _context.Accounts.ToListAsync();

            var viewModel = new AccountsViewModel
            {
                Accounts = accounts
            };

            return View(viewModel);
        }

        // GET: Accounts/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Accounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CreateAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _context.CreateAccountAsync(model.CustomerId, model.AccountNumber, model.Balance);
                return RedirectToAction("Index", "Account");
            }
            return View(model);
        }

        [HttpGet("Account/Login")]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("Account/Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Username == "admin" && model.Password == "password")
                {
                    var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, model.Username)
                        };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            return View(model);
        }

        // POST: Account/Logout
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
