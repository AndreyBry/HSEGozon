using HSEGozon.OrdersService.Abstractions;
using HSEGozon.OrdersService.Api.Filters;
using HSEGozon.OrdersService.Application;
using HSEGozon.OrdersService.Infrastructure.BackgroundServices;
using HSEGozon.OrdersService.Infrastructure.Data;
using HSEGozon.OrdersService.Infrastructure.Messaging;
using HSEGozon.OrdersService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                .ToList();

            return new BadRequestObjectResult(new
            {
                error = "Validation failed",
                details = errors,
                example = new { 
                    userId = "123e4567-e89b-12d3-a456-426614174000",
                    amount = 500.00,
                    description = "Ноутбук ASUS ROG Strix"
                }
            });
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Orders Service API", 
        Version = "v1",
        Description = "API для управления заказами пользователей"
    });
    
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    
    c.EnableAnnotations();
    
    c.OperationFilter<SwaggerExampleOperationFilter>();
    
    c.ExampleFilters();
    
    c.UseInlineDefinitionsForEnums();
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=orders-db;Port=5432;Database=orders;Username=postgres;Password=postgres";

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IMessagePublisher, RabbitMqMessagePublisher>();
builder.Services.AddScoped<IMessageConsumer, RabbitMqMessageConsumer>();

builder.Services.AddSingleton<IRabbitMqConnection>(sp =>
{
    var config = builder.Configuration;
    return new RabbitMqConnection(
        config["RabbitMQ:HostName"] ?? "rabbitmq",
        config["RabbitMQ:Port"] ?? "5672",
        config["RabbitMQ:UserName"] ?? "guest",
        config["RabbitMQ:Password"] ?? "guest"
    );
});

builder.Services.AddHostedService<OutboxProcessorService>();
builder.Services.AddHostedService<PaymentStatusConsumerService>();

builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (System.Text.Json.JsonException ex)
    {
        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
        {
            error = "Invalid JSON format",
            message = "Expected JSON object, but received invalid format",
            details = ex.Message,
            example = new { 
                userId = "123e4567-e89b-12d3-a456-426614174000",
                amount = 500.00,
                description = "Ноутбук ASUS ROG Strix"
            }
        }));
    }
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DefaultModelsExpandDepth(-1);
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders Service API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Log.Information("Orders Service starting...");

app.Run();

