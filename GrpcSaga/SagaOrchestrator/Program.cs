var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpcClient<OrderService.OrderService.OrderServiceClient>();
builder.Services.AddGrpcClient<PaymentService.PaymentService.PaymentServiceClient>();

var app = builder.Build();
app.Run();