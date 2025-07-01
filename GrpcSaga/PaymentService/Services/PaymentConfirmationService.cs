using PaymentGrpc;

namespace PaymentService.Services;

public class PaymentConfirmationService : IPaymentConfirmationService
{
    public Task<bool> ConfirmPaymentAsync(CardInfo cardInfo)
    {
        var random = new Random();

        return Task.FromResult(random.Next(0, 2) == 0);
    }
}