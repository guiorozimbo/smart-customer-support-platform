using CustomerSupportPlatform.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPlatformApiObservability();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Customer Support Platform - API Gateway", Version = "v1" });
});

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

app.UseHttpsRedirection();
app.MapReverseProxy();

app.MapGet("/", () => Results.Ok(new
{
    name = "Customer Support Platform - API Gateway",
    version = "1.0",
    endpoints = new[] { "/api/orders", "/api/customers", "/api/tickets", "/api/chatbot", "/api/integration" }
})).WithName("Health");

app.Run();
