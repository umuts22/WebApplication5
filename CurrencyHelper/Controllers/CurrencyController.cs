using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CurrencyHelper.Services;

namespace CurrencyHelper.Controllers
{
    public class CurrencyController : Controller
    {
        private readonly DataRecordService _dataRecordService;

        // Inject DataRecordService using constructor injection
        public CurrencyController(DataRecordService dataRecordService)
        {
            _dataRecordService = dataRecordService;
        }

        // Make the Index method asynchronous
        public async Task<IActionResult> Index()
        {
            var currencies = await _dataRecordService.GetCurrencyDataAsync();

            if (currencies == null)
            {
                // Handle the case where data retrieval fails
                return View("Error");
            }
            return View(currencies);
        }
    }
}
