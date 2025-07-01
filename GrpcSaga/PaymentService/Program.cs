using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IPaymentConfirmationService, PaymentConfirmationService>();

builder.Services.AddGrpc();
builder.Services.AddDbContext<AppDbContext>(optionsBuilder =>
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("OrdersDatabase")));

var app = builder.Build();
app.MapGrpcService<PaymentService.Services.GrpcServices.PaymentService>();

app.Run();