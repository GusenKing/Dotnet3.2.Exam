using Microsoft.EntityFrameworkCore;
using OrderService.Data;

namespace OrderService.Services;

public class ProductsService(AppDbContext dbContext) : IProductsService
{
    public async Task<IResult> GetAllProducts()
    {
        return Results.Ok(await dbContext.Products.AsNoTracking().ToListAsync());
    }
}