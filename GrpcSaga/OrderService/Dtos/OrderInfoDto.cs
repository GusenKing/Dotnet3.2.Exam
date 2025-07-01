namespace OrderService.Dtos;

public class OrderInfoDto
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public decimal TotalPrice { get; set; }
}