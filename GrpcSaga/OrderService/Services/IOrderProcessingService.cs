using OrderService.Dtos;

namespace OrderService.Services;

public interface IOrderProcessingService
{
    public Task<bool> ProcessOrderAndEnsurePayment(OrderRequest request);
}