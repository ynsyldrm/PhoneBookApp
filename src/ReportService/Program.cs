using Microsoft.EntityFrameworkCore;
using ReportService.Data;
using ReportService.Services;
using ReportService.BackgroundServices;
using Shared.Interfaces;
using Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddDbContext<ReportDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Business Services
builder.Services.AddScoped<IReportService, ReportService.Services.ReportService>();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddSingleton<IKafkaConsumer, KafkaConsumer>();
builder.Services.AddHttpClient<IContactServiceClient, ContactServiceClient>(client =>
{
  client.BaseAddress = new Uri(builder.Configuration["ContactService:BaseUrl"]!);
});

// Background Services
builder.Services.AddHostedService<ReportProcessingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// Auto migrate
using (var scope = app.Services.CreateScope())
{
  var context = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
  context.Database.Migrate();
}

app.Run();