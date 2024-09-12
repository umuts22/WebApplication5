using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using WebApplication5.Models;
using WebApplication5.ViewModels;
using Newtonsoft.Json;

namespace WebApplication5.Controllers
{
    public class CurrencyExchangeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public CurrencyExchangeController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IActionResult> Index()
        {
            var model = new CurrencyExchangeViewModel
            {
                CurrencyData = await GetCurrencyDataAsync()
            };

            return View("Index", model);
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
                ConvertedValue = result.Response.Value,
                CurrencyData = await GetCurrencyDataAsync() // Ensure currency data is included
            };

            return View("Index", model);
        }

        private async Task<List<CurrencyExchangeViewModel.CurrencyExchangeData>> GetCurrencyDataAsync()
        {
            var currencyData = new List<CurrencyExchangeViewModel.CurrencyExchangeData>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("dbo.GetCurrencyData", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    connection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            currencyData.Add(new CurrencyExchangeViewModel.CurrencyExchangeData
                            {
                                CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
                                CurrencyName = reader.GetString(reader.GetOrdinal("CurrencyName")),
                                ForexBuying = reader.GetDecimal(reader.GetOrdinal("ForexBuying")),
                                ForexSelling = reader.GetDecimal(reader.GetOrdinal("ForexSelling")),
                                BanknoteBuying = reader.GetDecimal(reader.GetOrdinal("BanknoteBuying")),
                                BanknoteSelling = reader.GetDecimal(reader.GetOrdinal("BanknoteSelling")),
                                RecordDate = reader.GetDateTime(reader.GetOrdinal("RecordDate"))
                            });
                        }
                    }
                }
            }

            return currencyData;
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
}
