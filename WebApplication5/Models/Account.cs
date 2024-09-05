namespace WebApplication5.Models
{
    public class Account
    {
        public int AccountID { get; set; }
        public int CustomerID { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }

        public Customer Customer { get; set; }
    }

}
