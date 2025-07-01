namespace PaymentService.Data.Models;

public class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public PaymentStatus Status { get; set; }
}