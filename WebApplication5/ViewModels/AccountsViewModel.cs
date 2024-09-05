using WebApplication5.Models;
namespace WebApplication5.ViewModels

{

    public class AccountsViewModel
    {
        

        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public List<Account> Accounts { get; set; }
    }
}
