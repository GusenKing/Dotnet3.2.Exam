using Google.Protobuf;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Data.Models;
using OrderService.Dtos;
using PaymentGrpc;

namespace OrderService.Services;

public class OrderProcessingService(
    ILogger<OrderProcessingService> logger,
    AppDbContext dbContext,
    PaymentService.PaymentServiceClient paymentServiceClient) : IOrderProcessingService
{
    public async Task<IResult> ProcessOrderAndEnsurePayment(OrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var orderProcessingResult = await ProcessOrderAsync(request.OrderInfo, cancellationToken);
        if (orderProcessingResult == null) return Results.BadRequest("Requested product is not available");

        var orderId = orderProcessingResult.Value;
        var paymentProcessingResult = await paymentServiceClient.ProcessPaymentAsync(new ProcessPaymentRequest
        {
            OrderId = ByteString.CopyFrom(orderId.ToByteArray()),
            Amount = request.OrderInfo.Amount,
            CardInfo = new CardInfo
            {
                Number = request.PaymentInfo.Number,
                Holder = request.PaymentInfo.Holder,
                Month = request.PaymentInfo.Month,
                Year = request.PaymentInfo.Year,
                SecurityCode = request.PaymentInfo.SecurityCode
            },
            CustomerId = ByteString.CopyFrom(request.OrderInfo.UserId.ToByteArray())
        }, cancellationToken: cancellationToken);

        if (paymentProcessingResult.IsSuccessful)
            return Results.Ok(orderId);

        await OrderProcessingRollbackAsync(orderId, cancellationToken);
        return Results.Problem("Payment was cancelled or could not be processed");
    }

    private async Task<Guid?> ProcessOrderAsync(OrderInfoDto orderInfo, CancellationToken cancellationToken = default)
    {
        var requestedProduct =
            await dbContext.Products.FirstOrDefaultAsync(product => product.Id == orderInfo.ProductId,
                cancellationToken);

        if (requestedProduct == null) return null;

        if (requestedProduct.ProductQuantity <= 0) return null;

        requestedProduct.ProductQuantity -= 1;
        try
        {
            var newOrder = await dbContext.Orders.AddAsync(new Order
            {
                CustomerId = orderInfo.UserId,
                ProductId = orderInfo.ProductId,
                OrderDate = DateTime.UtcNow,
                Amount = orderInfo.Amount
            }, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return newOrder.Entity.Id;
        }
        catch (Exception e)
        {
            logger.LogError("Error occured while processing order, Details: {errorMessage}", e.Message);
            return null;
        }
    }

    private async Task OrderProcessingRollbackAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var savedOrder = await dbContext.Orders.FirstAsync(order => order.Id == orderId, cancellationToken);
        var affectedProduct =
            await dbContext.Products.FirstOrDefaultAsync(product => product.Id == savedOrder.ProductId,
                cancellationToken);
        if (affectedProduct == null)
        {
            logger.LogError("Order was for product that doesn't exist");
            return;
        }

        savedOrder.IsCancelled = true;
        affectedProduct!.ProductQuantity += 1;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}