using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Interfaces;
using System.Text.Json;

namespace Shared.Services;

public class KafkaProducer : IKafkaProducer, IDisposable
{
  private readonly IProducer<string, string> _producer;
  private readonly ILogger<KafkaProducer> _logger;

  public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
  {
    _logger = logger;

    var config = new ProducerConfig
    {
      BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
      MessageSendMaxRetries = 3,
      RetryBackoffMs = 1000,
      Acks = Acks.All
    };

    _producer = new ProducerBuilder<string, string>(config)
        .SetErrorHandler((_, e) => _logger.LogError("Kafka producer error: {Error}", e.Reason))
        .Build();
  }

  public async Task ProduceAsync(string topic, string message)
  {
    try
    {
      var result = await _producer.ProduceAsync(topic, new Message<string, string>
      {
        Key = Guid.NewGuid().ToString(),
        Value = message
      });

      _logger.LogInformation("Message sent to Kafka. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
          topic, result.Partition, result.Offset);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to send message to Kafka topic: {Topic}", topic);
      throw;
    }
  }

  public async Task ProduceAsync<T>(string topic, T message) where T : class
  {
    var jsonMessage = JsonSerializer.Serialize(message);
    await ProduceAsync(topic, jsonMessage);
  }

  public void Dispose()
  {
    _producer?.Dispose();
  }
}