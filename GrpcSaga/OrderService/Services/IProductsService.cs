namespace OrderService.Services;

public interface IProductsService
{
    public Task<IResult> GetAllProducts();
}