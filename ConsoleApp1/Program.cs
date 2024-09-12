using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;

namespace CurrencyFetcher
{
    public class CurrencyDataFetcher
    {

        private static readonly HttpClient client = new HttpClient();

        // Entry point for fetching and parsing currency data
        public async Task<Dictionary<string, Dictionary<string, decimal>>> FetchCurrencyDataAsync(string currency = null, string date = null, string dataType = null)
        {
            var apiUrl = GetApiUrl(date);
            if (apiUrl == null) throw new Exception("Invalid date.");

            if (currency != null && !IsValidCurrency(currency)) throw new Exception("Invalid currency code.");

            string encoding = GetEncoding(date);

            try
            {
                var response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                var xml = await response.Content.ReadAsStringAsync();

                var xmlDocument = XDocument.Parse(xml);
                var currencies = ParseXmlToDictionary(xmlDocument);

                if (currency != null)
                {
                    var currencyData = currencies.ContainsKey(currency) ? currencies[currency] : null;
                    if (currencyData != null && dataType != null)
                    {
                        if (currencyData.ContainsKey(dataType))
                        {
                            return new Dictionary<string, Dictionary<string, decimal>> { { currency, new Dictionary<string, decimal> { { dataType, currencyData[dataType] } } } };
                        }
                        else
                        {
                            throw new Exception("Invalid data type.");
                        }
                    }
                    return new Dictionary<string, Dictionary<string, decimal>> { { currency, currencyData } };
                }

                if (dataType != null)
                {
                    var result = new Dictionary<string, Dictionary<string, decimal>>();
                    foreach (var pair in currencies)
                    {
                        if (pair.Value.ContainsKey(dataType))
                        {
                            result[pair.Key] = new Dictionary<string, decimal> { { dataType, pair.Value[dataType] } };
                        }
                    }
                    return result;
                }

                return currencies;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching or processing data.", ex);
            }
        }

        private string GetApiUrl(string date)
        {
            string baseUrl = "http://www.tcmb.gov.tr/kurlar/";
            string todayUrl = baseUrl + "today.xml";

            if (string.IsNullOrEmpty(date) || date.Equals("today", StringComparison.OrdinalIgnoreCase))
                return todayUrl;

            var parsedDate = ParseDate(date);
            if (parsedDate == null) return null;

            return $"{baseUrl}{parsedDate[2]}{parsedDate[1]}/{parsedDate[0]}{parsedDate[1]}{parsedDate[2]}.xml";
        }

        private string GetEncoding(string date)
        {
            DateTime firstUtfDate = new DateTime(2016, 8, 8);
            var currentDate = ParseDate(date);
            if (currentDate == null) return "ISO-8859-9";

            DateTime dateToCheck = new DateTime(int.Parse(currentDate[2]), int.Parse(currentDate[1]), int.Parse(currentDate[0]));
            return dateToCheck >= firstUtfDate ? "UTF-8" : "ISO-8859-9";
        }

        private bool IsValidCurrency(string currency)
        {
            return currency != null && currency.Length == 3;
        }

        private string[] ParseDate(string date)
        {
            if (DateTime.TryParseExact(date, new[] { "dd-MM-yyyy", "dd.MM.yyyy", "dd/MM/yyyy" }, null, DateTimeStyles.None, out var parsedDate))
            {
                return new[] { parsedDate.Day.ToString("D2"), parsedDate.Month.ToString("D2"), parsedDate.Year.ToString() };
            }

            return null;
        }

        private Dictionary<string, Dictionary<string, decimal>> ParseXmlToDictionary(XDocument xmlDocument)
        {
            var currencies = new Dictionary<string, Dictionary<string, decimal>>();

            foreach (var currencyElement in xmlDocument.Descendants("Currency"))
            {
                var currencyCode = currencyElement.Attribute("CurrencyCode")?.Value;
                if (currencyCode == null) continue;

                var currencyData = new Dictionary<string, decimal>();
                foreach (var dataElement in currencyElement.Elements())
                {
                    var dataType = dataElement.Name.LocalName;
                    if (decimal.TryParse(dataElement.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                    {
                        currencyData[dataType] = value;
                    }
                }

                currencies[currencyCode] = currencyData;
            }

            return currencies;
        }

        public async Task SaveCurrencyDataToDatabaseAsync(Dictionary<string, Dictionary<string, decimal>> currencyData)
        {
            string connectionString = "Server=UMUT\\DEMOSQL;Database=BankingApp;Trusted_Connection=True;TrustServerCertificate=True;Integrated Security=true"; // Update with your connection string

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                foreach (var currency in currencyData)
                {
                    var currencyCode = currency.Key;
                    var currencyName = "Unknown"; // Default value; adjust as needed
                    var forexBuying = currency.Value.ContainsKey("ForexBuying") ? currency.Value["ForexBuying"] : 0;
                    var forexSelling = currency.Value.ContainsKey("ForexSelling") ? currency.Value["ForexSelling"] : 0;
                    var banknoteBuying = currency.Value.ContainsKey("BanknoteBuying") ? currency.Value["BanknoteBuying"] : 0;
                    var banknoteSelling = currency.Value.ContainsKey("BanknoteSelling") ? currency.Value["BanknoteSelling"] : 0;

                    var query = @"
                INSERT INTO TcmbExchangeRates (CurrencyCode, CurrencyName, ForexBuying, ForexSelling, BanknoteBuying, BanknoteSelling, RecordDate)
                VALUES (@CurrencyCode, @CurrencyName, @ForexBuying, @ForexSelling, @BanknoteBuying, @BanknoteSelling, @RecordDate)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CurrencyCode", currencyCode);
                        command.Parameters.AddWithValue("@CurrencyName", currencyName); 
                        command.Parameters.AddWithValue("@ForexBuying", forexBuying);
                        command.Parameters.AddWithValue("@ForexSelling", forexSelling);
                        command.Parameters.AddWithValue("@BanknoteBuying", banknoteBuying);
                        command.Parameters.AddWithValue("@BanknoteSelling", banknoteSelling);
                        command.Parameters.AddWithValue("@RecordDate", DateTime.Today);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }

    }
    class Program
    {
        static async Task Main(string[] args)
        {
            var fetcher = new CurrencyDataFetcher();

            try
            {
                var result = await fetcher.FetchCurrencyDataAsync(null, "today", null);

                // Save the fetched data to SQL Server
                await fetcher.SaveCurrencyDataToDatabaseAsync(result);

                Console.WriteLine("Currency data successfully saved to database.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
