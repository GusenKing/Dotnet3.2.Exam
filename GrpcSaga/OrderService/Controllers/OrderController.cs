using Microsoft.AspNetCore.Mvc;
using OrderService.Dtos;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController(IProductsService productsService, IOrderProcessingService orderProcessingService)
    : ControllerBase
{
    [HttpGet]
    [Route("/products")]
    public async Task<IResult> GetProducts()
    {
        return await productsService.GetAllProducts();
    }

    [HttpPost]
    [Route("/place-order")]
    public async Task<IResult> PlaceOrder([FromBody] OrderRequest order)
    {
        return await orderProcessingService.ProcessOrderAndEnsurePayment(order);
    }
}