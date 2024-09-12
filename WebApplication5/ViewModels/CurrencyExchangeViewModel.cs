
namespace WebApplication5.ViewModels
{
    public class CurrencyExchangeViewModel
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal ConvertedValue { get; set; }
        public List<CurrencyExchangeData> CurrencyData { get; set; }

        // Properties for the graph data
        public List<DateTime> RecordDates { get; set; }
        public List<decimal> ForexBuyingRates { get; set; }
        public List<decimal> ForexSellingRates { get; set; }

        public CurrencyExchangeViewModel()
        {
            RecordDates = new List<DateTime>();
            ForexBuyingRates = new List<decimal>();
            ForexSellingRates = new List<decimal>();
        }


        public class CurrencyExchangeData
        {
            public string CurrencyCode { get; set; }
            public string CurrencyName { get; set; }
            public decimal ForexBuying { get; set; }
            public decimal ForexSelling { get; set; }
            public decimal BanknoteBuying { get; set; }
            public decimal BanknoteSelling { get; set; }
            public DateTime RecordDate { get; set; }
        }




    }
}
