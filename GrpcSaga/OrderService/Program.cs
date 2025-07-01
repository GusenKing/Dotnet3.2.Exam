using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Services;
using PaymentGrpc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddScoped<IProductsService, ProductsService>();
builder.Services.AddScoped<IOrderProcessingService, OrderProcessingService>();

builder.Services.AddDbContext<AppDbContext>(optionsBuilder =>
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("OrdersDatabase")));
builder.Services.AddGrpcClient<PaymentService.PaymentServiceClient>(options =>
    options.Address = new Uri(builder.Configuration["PaymentServiceGrpcAddress"]!));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

var context = services.GetRequiredService<AppDbContext>();
if (context.Database.GetPendingMigrations().Any())
    context.Database.Migrate();

app.Run();