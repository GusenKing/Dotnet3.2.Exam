using Grpc.Core;
using PaymentGrpc;
using PaymentService.Data;
using PaymentService.Data.Models;

namespace PaymentService.Services.GrpcServices;

public class PaymentService(
    AppDbContext dbContext,
    ILogger<PaymentService> logger,
    IPaymentConfirmationService paymentConfirmationService)
    : PaymentGrpc.PaymentService.PaymentServiceBase
{
    public override async Task<ProcessPaymentResponse> ProcessPayment(ProcessPaymentRequest request,
        ServerCallContext context)
    {
        var confirmationResult = await paymentConfirmationService.ConfirmPaymentAsync(request.CardInfo);

        await dbContext.AddAsync(new Payment
        {
            OrderId = new Guid(request.OrderId.ToByteArray()),
            CustomerId = new Guid(request.CustomerId.ToByteArray()),
            Amount = request.Amount,
            Date = DateTime.UtcNow,
            Status = confirmationResult ? PaymentStatus.Confirmed : PaymentStatus.Cancelled
        });
        await dbContext.SaveChangesAsync();

        return new ProcessPaymentResponse { IsSuccessful = confirmationResult };
    }
}