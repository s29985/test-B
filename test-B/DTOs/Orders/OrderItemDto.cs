namespace test_B.DTOs.Orders;

public class OrderItemDto
{
    public ProductDto Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
