using Microsoft.AspNetCore.Mvc;
using WebApplication5.DAL;
using WebApplication5.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApplication5.Controllers
{
    public class CustomerController : Controller
    {
        private readonly BankingDbContext _context;

        public CustomerController(BankingDbContext context)
        {
            _context = context;
        }

        // GET: Customer
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers
                .FromSqlRaw("EXEC dbo.GetAllCustomers")
                .ToListAsync();

            return View(customers);
        }

        // GET: Customer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FromSqlInterpolated($"EXEC dbo.GetCustomerById @CustomerID = {id}")
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }
    }
}
