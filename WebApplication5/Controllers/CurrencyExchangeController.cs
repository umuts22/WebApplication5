using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApplication5.Models;
using WebApplication5.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;

namespace WebApplication5.Controllers
{
    public class CurrencyExchangeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CurrencyExchangeController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var model = new CurrencyExchangeViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConvertCurrency(string fromCurrency, string toCurrency, decimal amount)
        {
            var apiKey = _configuration["CurrencyBeaconApiKey"];
            var apiUrl = $"https://api.currencybeacon.com/v1/convert/?api_key={apiKey}&from={fromCurrency}&to={toCurrency}&amount={amount}";

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetStringAsync(apiUrl);

            var result = JsonConvert.DeserializeObject<CurrencyExchangeResponse>(response);

            var model = new CurrencyExchangeViewModel
            {
                FromCurrency = result.Response.From,
                ToCurrency = result.Response.To,
                Amount = result.Response.Amount,
                ConvertedValue = result.Response.Value
            };

            return View("Index", model);
        }
    }
}

public class CurrencyExchangeResponse
{
    public Meta Meta { get; set; }
    public Response Response { get; set; }
}

public class Meta
{
    public int Code { get; set; }
    public string Disclaimer { get; set; }
}

public class Response
{
    public long Timestamp { get; set; }
    public string Date { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public decimal Amount { get; set; }
    public decimal Value { get; set; }
}
