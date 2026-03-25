using CustomerSupportPlatform.Application.DTOs;
using CustomerSupportPlatform.Application.Interfaces;
using CustomerSupportPlatform.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Services;
using Xunit;

namespace OrderService.Tests;

/// <summary>
/// Unit tests for <see cref="OrderAppService"/> using mocked <see cref="IOrderRepository"/>.
/// </summary>
public class OrderAppServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ILogger<OrderAppService>> _loggerMock;
    private readonly OrderAppService _sut;

    public OrderAppServiceTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _loggerMock = new Mock<ILogger<OrderAppService>>();
        _sut = new OrderAppService(_orderRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderExists_ReturnsOrderDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = new Order
        {
            Id = id,
            OrderNumber = "ORD-001",
            TotalAmount = 99.99m,
            Status = "Pending",
            CustomerId = customerId,
            Description = "Test",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("ORD-001", result.OrderNumber);
        Assert.Equal(99.99m, result.TotalAmount);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal("Test", result.Description);
        _orderRepositoryMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        Assert.Null(result);
        _orderRepositoryMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByOrderNumberAsync_WhenOrderExists_ReturnsOrderDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var order = new Order
        {
            Id = id,
            OrderNumber = "ORD-123",
            TotalAmount = 150m,
            Status = "Shipped",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        _orderRepositoryMock
            .Setup(r => r.GetByOrderNumberAsync("ORD-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _sut.GetByOrderNumberAsync("ORD-123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("ORD-123", result.OrderNumber);
        Assert.Equal(150m, result.TotalAmount);
    }

    [Fact]
    public async Task GetByOrderNumberAsync_WhenOrderNumberIsNullOrWhiteSpace_ReturnsNull()
    {
        // Act
        var resultEmpty = await _sut.GetByOrderNumberAsync("");
        var resultNull = await _sut.GetByOrderNumberAsync("   ");

        // Assert
        Assert.Null(resultEmpty);
        Assert.Null(resultNull);
        _orderRepositoryMock.Verify(r => r.GetByOrderNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsCreatedOrderDto()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new CreateOrderRequest("ORD-NEW", 199.99m, customerId, "New order");
        Order? capturedOrder = null;
        _orderRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((o, _) => capturedOrder = o)
            .ReturnsAsync((Order o, CancellationToken _) => o);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ORD-NEW", result.OrderNumber);
        Assert.Equal(199.99m, result.TotalAmount);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal("Pending", result.Status);
        Assert.NotNull(capturedOrder);
        Assert.Equal("ORD-NEW", capturedOrder.OrderNumber);
        _orderRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateAsync(null!));
        _orderRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenOrderExists_UpdatesAndReturnsDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var order = new Order
        {
            Id = id,
            OrderNumber = "ORD-1",
            TotalAmount = 50m,
            Status = "Pending",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _orderRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var request = new UpdateOrderRequest("Shipped", "Updated description");

        // Act
        var result = await _sut.UpdateAsync(id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Shipped", result.Status);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal("Shipped", order.Status);
        Assert.Equal("Updated description", order.Description);
        _orderRepositoryMock.Verify(r => r.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);
        var request = new UpdateOrderRequest("Shipped", null);

        // Act
        var result = await _sut.UpdateAsync(id, request);

        // Assert
        Assert.Null(result);
        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenOrderExists_ReturnsTrueAndCallsRepository()
    {
        // Arrange
        var id = Guid.NewGuid();
        var order = new Order { Id = id, OrderNumber = "ORD-1", CustomerId = Guid.NewGuid() };
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _orderRepositoryMock.Setup(r => r.DeleteAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        Assert.True(result);
        _orderRepositoryMock.Verify(r => r.DeleteAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenOrderDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _orderRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        Assert.False(result);
        _orderRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByCustomerIdAsync_ReturnsOrdersMappedToDtos()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orders = new List<Order>
        {
            new() { Id = Guid.NewGuid(), OrderNumber = "O1", CustomerId = customerId, TotalAmount = 10m },
            new() { Id = Guid.NewGuid(), OrderNumber = "O2", CustomerId = customerId, TotalAmount = 20m }
        };
        _orderRepositoryMock
            .Setup(r => r.GetByCustomerIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _sut.GetByCustomerIdAsync(customerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("O1", result[0].OrderNumber);
        Assert.Equal("O2", result[1].OrderNumber);
    }
}
