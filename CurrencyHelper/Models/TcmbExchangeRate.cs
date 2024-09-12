namespace CurrencyHelper.Models;
public class TcmbExchangeRate
{
    public int? Id { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; }
    public decimal?  ForexBuying { get; set; }
    public decimal? ForexSelling { get; set; }
    public decimal? BanknoteBuying { get; set; }
    public decimal? BanknoteSelling { get; set; }
    public DateTime RecordDate { get; set; }
}
