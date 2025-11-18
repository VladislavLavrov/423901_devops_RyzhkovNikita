using CalculatorApp.Data;
using CalculatorApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CalculatorApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly CalculatorContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(CalculatorContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new CalculatorModel
            {
                History = await _context.CalculationHistories
                    .OrderByDescending(h => h.CreatedAt)
                    .Take(10)
                    .ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Calculate(CalculatorModel model)
        {
            if (ModelState.IsValid)
            {
                model.Calculate();

                // —охран€ем в историю если вычисление успешно
                if (string.IsNullOrEmpty(model.ErrorMessage))
                {
                    await SaveToHistory(model);
                }
            }

            // «агружаем историю дл€ отображени€
            model.History = await _context.CalculationHistories
                .OrderByDescending(h => h.CreatedAt)
                .Take(10)
                .ToListAsync();

            return View("Index", model);
        }

        private async Task SaveToHistory(CalculatorModel model)
        {
            try
            {
                var history = new CalculationHistory
                {
                    Operand1 = model.FirstNumber,
                    Operation = model.Operation,
                    Operand2 = model.Operation == "sqrt" ? null : model.SecondNumber,
                    Result = model.Result,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CalculationHistories.Add(history);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving calculation history");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}