using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using test_B.DTOs.Orders;
using test_B.Services.Interfaces;

namespace test_B.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrdersService _ordersService;

    public OrdersController(IOrdersService ordersService)
    {
        _ordersService = ordersService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDetailsDto>> GetOrder(int id, CancellationToken cancellationToken)
    {
        var dto = await _ordersService.GetOrderAsync(id, cancellationToken);
        if (dto == null)
        {
            return NotFound(new { message = $"Order with id {id} not found" });
        }

        return Ok(dto);
    }

    [HttpPut]
    public async Task<IActionResult> ProcessOrder([FromBody] UpdateOrderRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            await _ordersService.ProcessOrderAsync(request, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
