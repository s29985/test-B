using test_B.DTOs.Orders;

namespace test_B.Services.Interfaces;

public interface IOrdersService
{
    Task<OrderDetailsDto?> GetOrderAsync(int id, CancellationToken cancellationToken);
    Task ProcessOrderAsync(UpdateOrderRequestDto request, CancellationToken cancellationToken);
}
