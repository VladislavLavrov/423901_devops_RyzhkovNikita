using Confluent.Kafka;
using Calculator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Calculator.Controllers;
using Calculator.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace Calculator.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly string _topic;
        private readonly IConsumer<Null, string> _kafkaConsumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _clientFactory;

        public KafkaConsumerService(IConfiguration config, IServiceProvider serviceProvider, IHttpClientFactory clientFactory)
        {
            var consumerConfig = new ConsumerConfig();
            config.GetSection("Kafka:ConsumerSettings").Bind(consumerConfig);
            _topic = config.GetValue<string>("Kafka:TopicName") ?? "calculator-ryzhkov";
            _kafkaConsumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
            _serviceProvider = serviceProvider;
            _clientFactory = clientFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        private async Task StartConsumerLoop(CancellationToken cancellationToken)
        {
            _kafkaConsumer.Subscribe(_topic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var cr = _kafkaConsumer.Consume(cancellationToken);

                    // Десериализация сообщения
                    var inputData = JsonSerializer.Deserialize<CalculationMessage>(cr.Message.Value);

                    if (inputData != null)
                    {
                        // Логируем полученное сообщение
                        Console.WriteLine($"Message received: {cr.Message.Value}");

                        // Создаем HTTP клиент для callback
                        var httpClient = _clientFactory.CreateClient();

                        // Отправляем данные обратно через callback
                        // Измените порт на свой (5013)
                        var response = await httpClient.PostAsJsonAsync($"http://localhost:5013/Home/Callback", inputData);

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Callback successful");
                        }
                        else
                        {
                            Console.WriteLine($"Callback failed: {response.StatusCode}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Consume error: {e.Error.Reason}");
                    if (e.Error.IsFatal)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unexpected error: {e.Message}");
                }
            }
        }

        public override void Dispose()
        {
            _kafkaConsumer.Close();
            _kafkaConsumer.Dispose();
            base.Dispose();
        }
    }
}