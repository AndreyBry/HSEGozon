using HSEGozon.ApiGateway.Api.Filters;
using Serilog;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "HSEGozon API Gateway", 
        Version = "v1",
        Description = "API Gateway для маршрутизации запросов к микросервисам"
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
});

builder.Services.AddHttpClient("OrdersService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:OrdersService"] ?? "http://orders-service:5000");
});

builder.Services.AddHttpClient("PaymentsService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:PaymentsService"] ?? "http://payments-service:5000");
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Log.Information("API Gateway starting...");

app.Run();

