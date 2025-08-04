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
      HeartbeatIntervalMs = 10000,
      Debug = "consumer,cgrp,topic,fetch",
      LogConnectionClose = false
    };
  }

  public async Task ConsumeAsync(string topic, Func<string, Task> messageHandler, CancellationToken cancellationToken)
  {
    using var consumer = new ConsumerBuilder<string, string>(_config)
        .SetErrorHandler((_, e) => _logger.LogError("Kafka consumer error: {Error}", e.Reason))
        .SetLogHandler((_, logMessage) =>
        {
          switch (logMessage.Level)
          {
            case SyslogLevel.Error:
              _logger.LogError("Kafka log: {Message}", logMessage.Message);
              break;
            case SyslogLevel.Warning:
              _logger.LogWarning("Kafka log: {Message}", logMessage.Message);
              break;
            default:
              _logger.LogDebug("Kafka log: {Message}", logMessage.Message);
              break;
          }
        })
        .SetPartitionsAssignedHandler((c, partitions) =>
        {
          _logger.LogInformation("Assigned partitions: {Partitions}",
                  string.Join(", ", partitions.Select(p => $"{p.Topic}[{p.Partition}]")));
        })
        .SetPartitionsRevokedHandler((c, partitions) =>
        {
          _logger.LogInformation("Revoked partitions: {Partitions}",
                  string.Join(", ", partitions.Select(p => $"{p.Topic}[{p.Partition}]")));
        })
        .Build();

    consumer.Subscribe(topic);
    _logger.LogInformation("Started consuming from topic: {Topic} with GroupId: {GroupId}",
        topic, _config.GroupId);

    // Wait for partition assignment
    var timeout = TimeSpan.FromSeconds(30);
    var startTime = DateTime.UtcNow;

    try
    {
      //while (!cancellationToken.IsCancellationRequested)
      //{
        try
        {
          // Use timeout to avoid indefinite blocking
          var consumeResult = consumer.Consume(TimeSpan.FromSeconds(5));

          if (consumeResult == null)
          {
            // Timeout occurred, log and continue
            _logger.LogDebug("No message received within timeout period from topic: {Topic}", topic);

            // Check if we've been waiting too long without any assignment
            if (DateTime.UtcNow - startTime > timeout && consumer.Assignment.Count == 0)
            {
              _logger.LogWarning("No partition assignment after {Timeout} seconds for topic: {Topic}",
                  timeout.TotalSeconds, topic);
            }
            //continue;
            return;
          }

          if (consumeResult.IsPartitionEOF)
          {
            _logger.LogDebug("Reached end of partition {Partition} for topic: {Topic}",
                consumeResult.Partition, topic);
            //continue;
          }

          if (consumeResult.Message?.Value != null)
          {
            _logger.LogInformation("Received message from topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                topic, consumeResult.Partition, consumeResult.Offset);

            await messageHandler(consumeResult.Message.Value);

            // Manual commit
            consumer.Commit(consumeResult);
            _logger.LogDebug("Message processed and committed successfully");
          }
          else
          {
            _logger.LogWarning("Received null message from topic: {Topic}", topic);
          }
        }
        catch (ConsumeException ex)
        {
          _logger.LogError(ex, "Error consuming message from topic: {Topic}", topic);

          // Add small delay to prevent tight loop on persistent errors
          await Task.Delay(1000, cancellationToken);
        }
        catch (OperationCanceledException)
        {
          _logger.LogInformation("Consumer operation cancelled for topic: {Topic}", topic);
          //break;
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error processing message from topic: {Topic}", topic);

          // Add small delay to prevent tight loop on persistent errors
          await Task.Delay(1000, cancellationToken);
        }
      //}
    }
    finally
    {
      try
      {
        consumer.Close();
        _logger.LogInformation("Stopped consuming from topic: {Topic}", topic);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error closing consumer for topic: {Topic}", topic);
      }
    }
  }

  public void Dispose()
  {
    // Consumer dispose işlemi ConsumeAsync içinde yapılıyor
  }
}