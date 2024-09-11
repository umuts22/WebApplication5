using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using WebApplication5.Controllers;
using WebApplication5.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using Moq.Protected;

namespace WebApplication5.Tests
{
    public class CurrencyExchangeControllerTests
    {
        private readonly CurrencyExchangeController _controller;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IConfiguration> _configurationMock;

        public CurrencyExchangeControllerTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _configurationMock = new Mock<IConfiguration>();

            _controller = new CurrencyExchangeController(_httpClientFactoryMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task ConvertCurrency_Returns_ConvertedValue()
        {
            // Arrange
            var fromCurrency = "USD";
            var toCurrency = "TRY";
            var amount = 1m;

            var apiKey = "test_api_key";
            var apiUrl = $"https://api.currencybeacon.com/v1/convert/?api_key={apiKey}&from={fromCurrency}&to={toCurrency}&amount={amount}";

            var mockResponse = new CurrencyExchangeResponse
            {
                Meta = new Meta { Code = 200, Disclaimer = "Test Disclaimer" },
                Response = new Response
                {
                    Timestamp = 1725969444,
                    Date = "2024-09-10",
                    From = fromCurrency,
                    To = toCurrency,
                    Amount = amount,
                    Value = 34.04866173m
                }
            };

            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(mockResponse), Encoding.UTF8, "application/json")
            };

            var mockHttpClient = new Mock<HttpMessageHandler>();
            mockHttpClient
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponseMessage);

            var client = new HttpClient(mockHttpClient.Object);

            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
            _configurationMock.Setup(c => c["CurrencyBeaconApiKey"]).Returns(apiKey);

            // Act
            var result = await _controller.ConvertCurrency(fromCurrency, toCurrency, amount) as ViewResult;
            var model = result?.Model as CurrencyExchangeViewModel;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(model);
            Assert.Equal(fromCurrency, model.FromCurrency);
            Assert.Equal(toCurrency, model.ToCurrency);
            Assert.Equal(amount, model.Amount);
            Assert.Equal(mockResponse.Response.Value, model.ConvertedValue);
        }
    }
}
