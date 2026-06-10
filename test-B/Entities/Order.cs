using System;
using System.Collections.Generic;

namespace test_B.Entities;

public class Order
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = null!;
    public decimal TotalAmount { get; set; }

    // Column name in ERD: Users_UserId
    public int UsersUserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
