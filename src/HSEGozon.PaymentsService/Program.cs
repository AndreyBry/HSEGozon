using HSEGozon.PaymentsService.Abstractions;
using HSEGozon.PaymentsService.Api.Filters;
using HSEGozon.PaymentsService.Application;
using HSEGozon.PaymentsService.Infrastructure.BackgroundServices;
using HSEGozon.PaymentsService.Infrastructure.Data;
using HSEGozon.PaymentsService.Infrastructure.Messaging;
using HSEGozon.PaymentsService.Infrastructure.Repositories;
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
                example = new { userId = "123e4567-e89b-12d3-a456-426614174000" }
            });
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Payments Service API", 
        Version = "v1",
        Description = "API для управления счетами и платежами пользователей"
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
    ?? "Host=payments-db;Port=5432;Database=payments;Username=postgres;Password=postgres";

builder.Services.AddDbContext<PaymentsDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
builder.Services.AddScoped<IInboxMessageRepository, InboxMessageRepository>();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();
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
builder.Services.AddHostedService<InboxProcessorService>();

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
            example = new { userId = "123e4567-e89b-12d3-a456-426614174000" }
        }));
    }
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DefaultModelsExpandDepth(-1);
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payments Service API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Log.Information("Payments Service starting...");

app.Run();

