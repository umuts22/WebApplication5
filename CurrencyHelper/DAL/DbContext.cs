using Microsoft.EntityFrameworkCore;
using CurrencyHelper.Models;
namespace CurrencyHelper.DAL
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<TcmbExchangeRate> TcmbExchangeRates { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}

