using PaymentGrpc;

namespace PaymentService.Services;

public interface IPaymentConfirmationService
{
    public Task<bool> ConfirmPaymentAsync(CardInfo cardInfo);
}