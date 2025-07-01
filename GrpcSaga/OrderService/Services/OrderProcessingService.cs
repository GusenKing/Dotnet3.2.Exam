using Google.Protobuf;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Data.Models;
using OrderService.Dtos;
using PaymentGrpc;

namespace OrderService.Services;

// сервис оркестрирующий сагу 
public class OrderProcessingService(
    ILogger<OrderProcessingService> logger,
    AppDbContext dbContext,
    PaymentService.PaymentServiceClient paymentServiceClient) : IOrderProcessingService
{
    public async Task<IResult> ProcessOrderAndEnsurePayment(OrderRequest request,
        CancellationToken cancellationToken = default)
    {
        // выполняется попытка создать и сохранить order
        var orderProcessingResult = await ProcessOrderAsync(request.OrderInfo, cancellationToken);
        if (orderProcessingResult == null) return Results.BadRequest("Requested product is not available");

        // при успехе по grpc вызывается микросервис PaymentService на обработку платежа
        var orderId = orderProcessingResult.Value;
        try
        {
            var paymentProcessingResult = await paymentServiceClient.ProcessPaymentAsync(new ProcessPaymentRequest
            {
                OrderId = ByteString.CopyFrom(orderId.ToByteArray()),
                Amount = request.OrderInfo.TotalPrice,
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
            // при успехе возвращаем ранее сохраненный id order-a
            if (paymentProcessingResult.IsSuccessful)
            {
                logger.LogInformation("Successfully created order {orderId} and confirmed payment for it", orderId);
                return Results.Ok(orderId);
            }

            // при каком-либо отказе другого микросервиса выполняем откат
            await OrderProcessingRollbackAsync(orderId, cancellationToken);
            return Results.Problem("Payment was cancelled");
        }
        catch (Exception e)
        {
            await OrderProcessingRollbackAsync(orderId, cancellationToken);
            logger.LogError("Error processing order: {errorMessage}", e.Message);
            return Results.Problem("Payment could not be processed");
        }
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
                Amount = orderInfo.TotalPrice
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

        // оставляем запись о заказе, но выставляем IsCancelled, поясняя что заказ был отменён по какой-то причине
        savedOrder.IsCancelled = true;
        // отменяем уменьшение количества продуктов в стоке
        affectedProduct!.ProductQuantity += 1;

        logger.LogInformation("Order {orderId} was cancelled", orderId);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}