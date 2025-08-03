namespace Shared.Interfaces;

public interface IKafkaConsumer
{
  Task ConsumeAsync(string topic, Func<string, Task> messageHandler, CancellationToken cancellationToken);
}