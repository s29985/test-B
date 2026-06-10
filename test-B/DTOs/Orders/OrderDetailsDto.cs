using System.Collections.Generic;

namespace test_B.DTOs.Orders;

public class OrderDetailsDto
{
    public int OrderId { get; set; }
    public string OrderDate { get; set; } = null!;
    public string Status { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public string User { get; set; } = null!;
    public List<PaymentDto> Payments { get; set; } = new();
    public List<OrderItemDto> OrderItems { get; set; } = new();
}
