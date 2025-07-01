using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Data.Models;
using OrderService.Dtos;

namespace OrderService.Services;

public class OrderProcessingService(
    ILogger<OrderProcessingService> logger,
    AppDbContext dbContext,
    PaymentService.PaymentService.PaymentServiceClient paymentServiceClient) : IOrderProcessingService
{
    public async Task<bool> ProcessOrderAndEnsurePayment(OrderRequest request)
    {
        var orderProcessingResult = await ProcessOrderAsync(request.OrderInfo);
        if (!orderProcessingResult) return false;

        return true;
    }

    private async Task<bool> ProcessOrderAsync(OrderInfoDto orderInfo)
    {
        var requestedProduct =
            await dbContext.Products.FirstOrDefaultAsync(product => product.Id == orderInfo.ProductId);

        if (requestedProduct == null) return false;

        if (requestedProduct.ProductQuantity <= 0) return false;

        requestedProduct.ProductQuantity -= 1;
        try
        {
            await dbContext.Orders.AddAsync(new Order
            {
                CustomerId = orderInfo.UserId,
                ProductId = orderInfo.ProductId,
                OrderDate = DateTime.Now,
                Amount = orderInfo.Amount
            });
            await dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            logger.LogError("Error occured while processing order, Details: {errorMessage}", e.Message);
            return false;
        }
    }
}