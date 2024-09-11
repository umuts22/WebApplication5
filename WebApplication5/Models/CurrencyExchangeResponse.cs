namespace WebApplication5.Models
{
    public class CurrencyExchangeResponse
    {
        public string Base { get; set; }
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; }
    }
}
