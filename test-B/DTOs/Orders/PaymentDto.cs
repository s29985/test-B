namespace test_B.DTOs.Orders;

public class PaymentDto
{
    public int PaymentId { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; } = null!;
}
