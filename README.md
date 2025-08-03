# Mikroservis Telefon Rehberi Uygulaması

## Gereksinimler
- .NET 8.0 SDK
- Docker ve Docker Compose
- PostgreSQL
- Apache Kafka

## Kurulum ve Çalıştırma

1. Repository'yi klonlayın

2. Docker Compose ile servisleri başlatın:
   docker-compose up -d

3. Veritabanı migration'ları çalıştırın:
cd src/ContactService
dotnet ef database update
cd ../ReportService
dotnet ef database update