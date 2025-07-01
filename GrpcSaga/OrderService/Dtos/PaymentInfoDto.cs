using System.ComponentModel.DataAnnotations;

namespace OrderService.Dtos;

public class PaymentInfoDto
{
    [RegularExpression(@"^[0-9]{13,19}$")] public string Number { get; set; } = null!;

    [RegularExpression(@"^[A-Z\s\-]+$")] public string Holder { get; set; } = null!;

    [Range(1, 12)] public int Month { get; set; }

    [Range(0, 99)] public int Year { get; set; }

    [RegularExpression(@"^[0-9]{3}$")] public string SecurityCode { get; set; } = null!;
}