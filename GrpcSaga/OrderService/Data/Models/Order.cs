namespace OrderService.Data.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Amount { get; set; }
}