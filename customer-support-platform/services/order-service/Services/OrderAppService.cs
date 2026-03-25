using CustomerSupportPlatform.Application.DTOs;
using CustomerSupportPlatform.Application.Interfaces;
using CustomerSupportPlatform.Domain.Constants;
using CustomerSupportPlatform.Domain.Entities;

namespace OrderService.Services;

/// <summary>
/// Application service for order operations. Delegates persistence to <see cref="IOrderRepository"/>.
/// </summary>
public class OrderAppService : IOrderAppService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderAppService> _logger;

    public OrderAppService(IOrderRepository orderRepository, ILogger<OrderAppService> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, ct);
        return order is null ? null : MapToDto(order);
    }

    public async Task<OrderDto?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return null;

        var order = await _orderRepository.GetByOrderNumberAsync(orderNumber.Trim(), ct);
        return order is null ? null : MapToDto(order);
    }

    public async Task<IReadOnlyList<OrderDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId, ct);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken ct = default)
    {
        var orders = await _orderRepository.GetAllAsync(ct);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var order = new Order
        {
            OrderNumber = request.OrderNumber,
            TotalAmount = request.TotalAmount,
            CustomerId = request.CustomerId,
            Description = request.Description,
            Status = OrderStatus.Pending
        };
        var created = await _orderRepository.AddAsync(order, ct);
        _logger.LogInformation("Order created: {OrderNumber}, Id: {Id}", created.OrderNumber, created.Id);
        return MapToDto(created);
    }

    public async Task<OrderDto?> UpdateAsync(Guid id, UpdateOrderRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var order = await _orderRepository.GetByIdAsync(id, ct);
        if (order is null)
            return null;

        if (request.Status is { } status)
            order.Status = status;
        if (request.Description is { } desc)
            order.Description = desc;

        await _orderRepository.UpdateAsync(order, ct);
        _logger.LogInformation("Order updated: {Id}", id);
        return MapToDto(order);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, ct);
        if (order is null)
            return false;

        await _orderRepository.DeleteAsync(order, ct);
        _logger.LogInformation("Order deleted: {Id}", id);
        return true;
    }

    private static OrderDto MapToDto(Order o) => new(
        o.Id, o.OrderNumber, o.TotalAmount, o.Status, o.CustomerId, o.Description, o.CreatedAt);
}
