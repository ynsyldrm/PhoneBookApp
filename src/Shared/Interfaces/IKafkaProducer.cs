﻿namespace Shared.Interfaces;

public interface IKafkaProducer
{
  Task ProduceAsync(string topic, string message);
  Task ProduceAsync<T>(string topic, T message) where T : class;
}