# Mikroservis Telefon Rehberi Uygulaması

## Gereksinimler
- .NET 8.0 SDK
- Docker ve Docker Compose
- PostgreSQL
- Apache Kafka
- XUnit

## Kurulum ve Çalıştırma

1. Repository'yi klonlayın

2. Docker Compose ile servisleri başlatın:
   docker-compose up -d

3. Veritabanı migration'ları çalıştırın:(Not: Manuel migration yapmaniza gerek yoktur.
Cunku auto migration runtime da yapilarak sizlerin testini kolaylastirmak adina bu adimi pas 
gecebilmeniz amaclanmistir.)
cd src/ContactService
dotnet ef database update
cd ../ReportService
dotnet ef database update

4. Docker ile uygulama servisleri ve imajlari sorunsuz ayaga kalktiktan sonra
Uygulama ana dizininine konumlanacak sekilde bir bash komut terminali(ben git bash kullandim)
uzerinden tum uygulamayi test edebilmek ve onizleyebilmek adina 'test-whole-app-script' isimli shell
scripti calistirabilirsiniz. Alternatif olarak ApiGateway servisi uzerinden postman(yada benzeri arac) kullanarak
uygulamada Contact, ContactInfo, Report olusturabilirsiniz. Post endpointlerine istekte bulunarak sureci baslatabilirsiniz.
Get endpointleri ile db ye kaydedilen kayitlari gorebilirsiniz.