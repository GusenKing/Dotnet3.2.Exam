using OrderService.Dtos;

namespace OrderService.Services;

public interface IOrderProcessingService
{
    public Task<IResult> ProcessOrderAndEnsurePayment(OrderRequest request,
        CancellationToken cancellationToken = default);
}