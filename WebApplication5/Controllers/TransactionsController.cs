using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApplication5.DAL;
using WebApplication5.ViewModels;

namespace WebApplication5.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly BankingDbContext _context;

        public TransactionsController(BankingDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(int accountId, decimal amount, string description, string createdBy)
        {
            try
            {
                await _context.DepositFundsAsync(accountId, amount, description, createdBy);
                TempData["SuccessMessage"] = "Deposit completed successfully.";
                return RedirectToAction("Success");
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Withdraw(int accountId, decimal amount, string description, string createdBy)
        {
            try
            {
                await _context.WithdrawFundsAsync(accountId, amount, description, createdBy);
                TempData["SuccessMessage"] = "Withdrawal completed successfully.";
                return RedirectToAction("Success");
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Transfer(int fromAccountId, int toAccountId, decimal amount)
        {
            if (fromAccountId == toAccountId)
            {
                TempData["ErrorMessage"] = "The source and destination accounts cannot be the same.";
                return RedirectToAction("Error");
            }

            try
            {
                await _context.TransferFundsAsync(fromAccountId, toAccountId, amount);
                TempData["SuccessMessage"] = "Transfer completed successfully.";
                return RedirectToAction("Success");
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Error");
            }
        }

        public IActionResult Success()
        {
            ViewData["SuccessMessage"] = TempData["SuccessMessage"];
            return View();
        }

        public IActionResult Error()
        {
            ViewData["ErrorMessage"] = TempData["ErrorMessage"];
            return View();
        }


        [HttpGet]
        public IActionResult Deposit()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Withdraw()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Transfer()
        {
            return View();
        }

        public IActionResult Index()
        {
            var model = new TransactionViewModel();
            return View(model);
        }
    }
}
