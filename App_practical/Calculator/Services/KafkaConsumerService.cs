using Confluent.Kafka;
using Calculator.Models;
using System.Text.Json;

namespace Calculator.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public KafkaConsumerService(IConfiguration configuration, ILogger<KafkaConsumerService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                GroupId = configuration["Kafka:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe("calculator-events");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);

                    if (consumeResult != null)
                    {
                        await ProcessMessage(consumeResult.Message.Value);
                        _logger.LogInformation($"Received message: {consumeResult.Message.Value}");
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError($"Error consuming message: {ex.Error.Reason}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unexpected error: {ex.Message}");
                }
            }
        }

        private async Task ProcessMessage(string messageJson)
        {
            try
            {
                var calculationMessage = JsonSerializer.Deserialize<CalculationMessage>(messageJson);

                if (calculationMessage != null)
                {
                    // Здесь можно добавить дополнительную обработку сообщений
                    // Например, сохранение в отдельную таблицу или отправку уведомлений
                    _logger.LogInformation($"Processed calculation: {calculationMessage.Operation} = {calculationMessage.Result}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message: {ex.Message}");
            }
        }

        public override void Dispose()
        {
            _consumer?.Close();
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}