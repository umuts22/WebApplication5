using System.Collections.Generic;
using WebApplication5.Models;

namespace WebApplication5.ViewModels
{
    public class TransferViewModel
    {
        public IEnumerable<Account> Accounts { get; set; }
        public int FromAccountId { get; set; }
        public int ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string TransferType { get; set; } // "Own" or "Other"
    }
}
