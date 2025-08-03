using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Interfaces;

namespace Shared.Services;

public class KafkaConsumer : IKafkaConsumer, IDisposable
{
  private readonly ConsumerConfig _config;
  private readonly ILogger<KafkaConsumer> _logger;

  public KafkaConsumer(IConfiguration configuration, ILogger<KafkaConsumer> logger)
  {
    _logger = logger;

    _config = new ConsumerConfig
    {
      BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
      GroupId = configuration["Kafka:GroupId"] ?? "phonebook-group",
      AutoOffsetReset = AutoOffsetReset.Earliest,
      EnableAutoCommit = false,
      SessionTimeoutMs = 30000,
      HeartbeatIntervalMs = 10000
    };
  }

  public async Task ConsumeAsync(string topic, Func<string, Task> messageHandler, CancellationToken cancellationToken)
  {
    using var consumer = new ConsumerBuilder<string, string>(_config)
        .SetErrorHandler((_, e) => _logger.LogError("Kafka consumer error: {Error}", e.Reason))
        .Build();

    consumer.Subscribe(topic);
    _logger.LogInformation("Started consuming from topic: {Topic}", topic);

    try
    {
      while (!cancellationToken.IsCancellationRequested)
      {
        try
        {
          var consumeResult = consumer.Consume(cancellationToken);

          if (consumeResult?.Message?.Value != null)
          {
            _logger.LogInformation("Received message from topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                topic, consumeResult.Partition, consumeResult.Offset);

            await messageHandler(consumeResult.Message.Value);

            // Manuel commit
            consumer.Commit(consumeResult);

            _logger.LogDebug("Message processed and committed successfully");
          }
        }
        catch (ConsumeException ex)
        {
          _logger.LogError(ex, "Error consuming message from topic: {Topic}", topic);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error processing message from topic: {Topic}", topic);
        }
      }
    }
    finally
    {
      consumer.Close();
      _logger.LogInformation("Stopped consuming from topic: {Topic}", topic);
    }
  }

  public void Dispose()
  {
    // Consumer dispose işlemi ConsumeAsync içinde yapılıyor
  }
}