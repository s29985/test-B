namespace test_B.Entities;

public class Payment
{
    public int PaymentId { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; } = null!;

    // Column name in ERD: Orders_OrderId
    public int OrdersOrderId { get; set; }
    public Order Order { get; set; } = null!;
}
