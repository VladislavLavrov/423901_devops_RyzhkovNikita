using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Calculator.Services
{
    public class KafkaProducerService : IDisposable
    {
        private readonly IProducer<Null, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;
        private readonly string _topic;

        public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
        {
            _logger = logger;

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:ProducerSettings:BootstrapServers"]
            };

            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
            _topic = configuration["Kafka:TopicName"] ?? "Ryzhkov";

            _logger.LogInformation($"Kafka Producer initialized for topic: {_topic}");
        }

        public async Task ProduceAsync(string message)
        {
            try
            {
                var result = await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = message });
                _logger.LogInformation($"Message delivered to {result.TopicPartitionOffset}");
            }
            catch (ProduceException<Null, string> ex)
            {
                _logger.LogError($"Delivery failed: {ex.Error.Reason}");
                throw;
            }
        }

        public async Task ProduceAsync(string topic, string message)
        {
            try
            {
                var result = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
                _logger.LogInformation($"Message delivered to {result.TopicPartitionOffset}");
            }
            catch (ProduceException<Null, string> ex)
            {
                _logger.LogError($"Delivery failed: {ex.Error.Reason}");
                throw;
            }
        }

        public void Dispose()
        {
            _producer?.Flush(TimeSpan.FromSeconds(5));
            _producer?.Dispose();
        }
    }
}