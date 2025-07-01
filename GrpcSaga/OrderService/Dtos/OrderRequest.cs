namespace OrderService.Dtos;

public record OrderRequest(OrderInfoDto OrderInfo, PaymentInfoDto PaymentInfo);