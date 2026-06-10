using Microsoft.EntityFrameworkCore;
using test_B.DTOs.Orders;
using test_B.Entities;
using test_B.Infrastructure;
using test_B.Services.Interfaces;

namespace test_B.Services;

public class OrdersService : IOrdersService
{
    private readonly AppDbContext _dbContext;

    public OrdersService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderDetailsDto?> GetOrderAsync(int id, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Payments)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id, cancellationToken);

        if (order == null)
        {
            return null;
        }

        var dto = new OrderDetailsDto
        {
            OrderId = order.OrderId,
            OrderDate = order.OrderDate.ToString("yyyy-MM-dd"),
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            User = order.User.Username,
            Payments = order.Payments
                .OrderBy(p => p.PaymentId)
                .Select(p => new PaymentDto
                {
                    PaymentId = p.PaymentId,
                    PaymentMethod = p.PaymentMethod,
                    Amount = p.Amount,
                    PaymentStatus = p.PaymentStatus
                }).ToList(),
            OrderItems = order.OrderItems
                .OrderBy(oi => oi.ProductId)
                .Select(oi => new OrderItemDto
                {
                    Product = new ProductDto
                    {
                        ProductId = oi.Product.ProductId,
                        Name = oi.Product.Name,
                        Description = oi.Product.Description,
                        Price = oi.Product.Price,
                        StockQuantity = oi.Product.StockQuantity
                    },
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
        };

        return dto;
    }

    public async Task ProcessOrderAsync(UpdateOrderRequestDto request, CancellationToken cancellationToken)
    {
        if (request.OrderId <= 0)
        {
            throw new ArgumentException("OrderId must be a positive integer.");
        }

        // Load order with related data needed for processing
        var order = await _dbContext.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with id {request.OrderId} was not found.");
        }

        // If there are any payments for this order, the operation should be aborted
        var hasPayments = await _dbContext.Payments
            .AsNoTracking()
            .AnyAsync(p => p.OrdersOrderId == request.OrderId, cancellationToken);
        if (hasPayments)
        {
            throw new InvalidOperationException("Cannot process order that has existing payments.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Mark order as processed
            order.Status = "Processed";

            // Identify product ids in this order
            var productIds = order.OrderItems.Select(oi => oi.ProductId).Distinct().ToList();

            // Load products to update prices
            var products = await _dbContext.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync(cancellationToken);

            foreach (var product in products)
            {
                var newPrice = Math.Round(product.Price * 0.9m, 2, MidpointRounding.AwayFromZero);
                product.Price = newPrice;
            }

            // Recalculate order total using NEW product prices and quantities
            var productPriceMap = products.ToDictionary(p => p.ProductId, p => p.Price);
            decimal total = 0m;
            foreach (var oi in order.OrderItems)
            {
                if (productPriceMap.TryGetValue(oi.ProductId, out var price))
                {
                    total += price * oi.Quantity;
                }
            }
            order.TotalAmount = Math.Round(total, 2, MidpointRounding.AwayFromZero);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
