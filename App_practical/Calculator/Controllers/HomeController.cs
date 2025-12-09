using Calculator.Data;
using Calculator.Models;
using Calculator.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Calculator.Controllers
{
    public class HomeController : Controller
    {
        private readonly CalculatorContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly KafkaProducerService _kafkaProducer;

        public HomeController(
            CalculatorContext context,
            ILogger<HomeController> logger,
            KafkaProducerService kafkaProducer)
        {
            _context = context;
            _logger = logger;
            _kafkaProducer = kafkaProducer;
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

                // Сохраняем в историю если вычисление успешно
                if (string.IsNullOrEmpty(model.ErrorMessage))
                {
                    await SaveToHistory(model);

                    // Отправляем событие в Kafka
                    await SendCalculationEvent(model);
                }
            }

            // Загружаем историю для отображения
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
                    Operand2 = model.Operation == "sqrt" ? (double?)null : model.SecondNumber,
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

        private async Task SendCalculationEvent(CalculatorModel model)
        {
            try
            {
                var message = new CalculationMessage
                {
                    Operation = model.Operation,
                    Operand1 = model.FirstNumber,
                    Operand2 = model.Operation == "sqrt" ? null : model.SecondNumber,
                    Result = model.Result,
                    Timestamp = DateTime.UtcNow
                };

                var jsonMessage = JsonSerializer.Serialize(message);
                await _kafkaProducer.ProduceAsync(jsonMessage);

                _logger.LogInformation($"Calculation event sent to Kafka: {model.Operation}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending calculation event to Kafka");
            }
        }

        [HttpPost]
        public IActionResult Callback([FromBody] CalculationMessage inputData)
        {
            try
            {
                _logger.LogInformation($"Callback received: {inputData.Operation} = {inputData.Result}");

                // Здесь можно обработать данные, полученные от консьюмера
                // Например, сохранить в другую таблицу или обновить UI

                return Ok(new { success = true, message = "Callback processed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Callback method");
                return StatusCode(500, new { success = false, error = ex.Message });
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