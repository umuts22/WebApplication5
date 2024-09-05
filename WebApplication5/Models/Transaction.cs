
namespace WebApplication5.Models
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public int AccountID { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } // e.g., 'Deposit', 'Withdrawal'
        public decimal Amount { get; set; }
        public string Description { get; set; }

        public int FromAccountID { get; set; }
        public string CreatedBy { get; set; }
        public string LastEditedBy { get; set; }
    }
}
