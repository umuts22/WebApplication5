using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CurrencyHelper.Models;

namespace CurrencyHelper.Services
{
    public class DataRecordService
    {
        private readonly string tcmbUrl = "https://www.tcmb.gov.tr/kurlar/today.xml";
        private readonly ILogger<DataRecordService> _logger;

        public DataRecordService(ILogger<DataRecordService> logger)
        {
            _logger = logger;
        }

        public async Task<List<TcmbExchangeRate>> GetCurrencyDataAsync()
        {
            var currencies = new List<TcmbExchangeRate>();

            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync(tcmbUrl);
                    _logger.LogInformation("Response from TCMB: {Response}", response);

                    var document = XDocument.Parse(response);
                    _logger.LogInformation("Parsed XML Document: {Document}", document);

                    foreach (var currency in document.Descendants("Currency"))
                    {
                        currencies.Add(new TcmbExchangeRate
                        {
                            CurrencyCode = currency.Attribute("CurrencyCode")?.Value,
                            CurrencyName = currency.Element("CurrencyName")?.Value,
                            ForexBuying = decimal.TryParse(currency.Element("ForexBuying")?.Value, out var forexBuying) ? forexBuying : 0,
                            ForexSelling = decimal.TryParse(currency.Element("ForexSelling")?.Value, out var forexSelling) ? forexSelling : 0,
                            BanknoteBuying = decimal.TryParse(currency.Element("BanknoteBuying")?.Value, out var banknoteBuying) ? banknoteBuying : 0,
                            BanknoteSelling = decimal.TryParse(currency.Element("BanknoteSelling")?.Value, out var banknoteSelling) ? banknoteSelling : 0,
                            RecordDate = DateTime.TryParse(document.Root.Element("Date")?.Value, out var recordDate) ? recordDate : DateTime.MinValue
                        });
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request error while fetching currency data.");
            }
            catch (XmlException xmlEx)
            {
                _logger.LogError(xmlEx, "XML parsing error while processing currency data.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching or parsing currency data.");
            }

            return currencies;
        }
    }
}
