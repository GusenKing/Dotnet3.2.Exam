namespace OrderService.Data.Models;

public class Product
{
    public Guid Id { get; set; }
    public required string ProductName { get; set; }
    public uint ProductQuantity { get; set; }
    public decimal ProductPrice { get; set; }
}