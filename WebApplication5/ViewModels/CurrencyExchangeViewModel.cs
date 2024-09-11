namespace WebApplication5.ViewModels
{
    public class CurrencyExchangeViewModel
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal ConvertedValue { get; set; }
    }

}
