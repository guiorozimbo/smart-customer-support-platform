using ChatbotIntegration.Integration;
using ChatbotIntegration.Middleware;
using ChatbotIntegration.Options;
using CustomerSupportPlatform.Application.Interfaces;
using CustomerSupportPlatform.Hosting;
using CustomerSupportPlatform.Infrastructure.Repositories;
using Database;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Customer Support — Integração HTTP (chat)", Version = "v1" });
});

// Um único ponto de configuração JSON para a Minimal API (serialização nativa com tipos gerados).
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolver = ChatIntegrationJsonContext.Default;
});

builder.Services.Configure<ChatbotSecurityOptions>(
    builder.Configuration.GetSection(ChatbotSecurityOptions.SectionName));

builder.Services.AddPlatformApiObservability();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database", tags: ["db", "ready"]);

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UsePlatformRequestLogging();
app.UseMiddleware<ChatbotApiKeyMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapChatIntegrationEndpoints();
app.MapControllers();

app.Run();
