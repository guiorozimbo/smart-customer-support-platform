using CustomerSupportPlatform.Application.DTOs;
using CustomerSupportPlatform.Application.Interfaces;
using CustomerSupportPlatform.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using TicketService.Services;
using Xunit;

namespace TicketService.Tests;

/// <summary>
/// Unit tests for <see cref="TicketAppService"/> using mocked <see cref="ITicketRepository"/>.
/// </summary>
public class TicketAppServiceTests
{
    private readonly Mock<ITicketRepository> _ticketRepositoryMock;
    private readonly Mock<ILogger<TicketAppService>> _loggerMock;
    private readonly TicketAppService _sut;

    public TicketAppServiceTests()
    {
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _loggerMock = new Mock<ILogger<TicketAppService>>();
        _sut = new TicketAppService(_ticketRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesTicketAndReturnsDto()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var request = new CreateTicketRequest(
            "Cannot track my order",
            "Order ORD-123 has not arrived.",
            customerId,
            orderId,
            "High");
        SupportTicket? capturedTicket = null;
        _ticketRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SupportTicket>(), It.IsAny<CancellationToken>()))
            .Callback<SupportTicket, CancellationToken>((t, _) => capturedTicket = t)
            .ReturnsAsync((SupportTicket t, CancellationToken _) => t);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Cannot track my order", result.Title);
        Assert.Equal("Order ORD-123 has not arrived.", result.Description);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal("High", result.Priority);
        Assert.Equal("Open", result.Status);
        Assert.NotNull(capturedTicket);
        Assert.Equal("Cannot track my order", capturedTicket.Title);
        Assert.Equal("Open", capturedTicket.Status);
        _ticketRepositoryMock.Verify(r => r.AddAsync(It.IsAny<SupportTicket>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenPriorityIsNull_UsesDefaultMedium()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = new CreateTicketRequest("Title", "Description", customerId, null, null);
        SupportTicket? capturedTicket = null;
        _ticketRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SupportTicket>(), It.IsAny<CancellationToken>()))
            .Callback<SupportTicket, CancellationToken>((t, _) => capturedTicket = t)
            .ReturnsAsync((SupportTicket t, CancellationToken _) => t);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Medium", result.Priority);
        Assert.NotNull(capturedTicket);
        Assert.Equal("Medium", capturedTicket.Priority);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateAsync(null!));
        _ticketRepositoryMock.Verify(r => r.AddAsync(It.IsAny<SupportTicket>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTicketExists_ReturnsTicketDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var ticket = new SupportTicket
        {
            Id = id,
            Title = "Login issue",
            Description = "Cannot log in",
            Status = "Open",
            Priority = "High",
            CustomerId = customerId,
            OrderId = null,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };
        _ticketRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Login issue", result.Title);
        Assert.Equal("Cannot log in", result.Description);
        Assert.Equal("Open", result.Status);
        Assert.Equal("High", result.Priority);
        Assert.Equal(customerId, result.CustomerId);
        _ticketRepositoryMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTicketDoesNotExist_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _ticketRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportTicket?)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        Assert.Null(result);
        _ticketRepositoryMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByCustomerIdAsync_ReturnsTicketsMappedToDtos()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var tickets = new List<SupportTicket>
        {
            new() { Id = Guid.NewGuid(), Title = "T1", Description = "D1", CustomerId = customerId, Status = "Open", Priority = "Low" },
            new() { Id = Guid.NewGuid(), Title = "T2", Description = "D2", CustomerId = customerId, Status = "Resolved", Priority = "High" }
        };
        _ticketRepositoryMock
            .Setup(r => r.GetByCustomerIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tickets);

        // Act
        var result = await _sut.GetByCustomerIdAsync(customerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("T1", result[0].Title);
        Assert.Equal("T2", result[1].Title);
        Assert.Equal("Open", result[0].Status);
        Assert.Equal("Resolved", result[1].Status);
    }

    [Fact]
    public async Task GetByStatusAsync_WhenStatusIsValid_ReturnsTickets()
    {
        // Arrange
        var tickets = new List<SupportTicket>
        {
            new() { Id = Guid.NewGuid(), Title = "Open 1", Status = "Open", CustomerId = Guid.NewGuid(), Priority = "Medium" }
        };
        _ticketRepositoryMock
            .Setup(r => r.GetByStatusAsync("Open", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tickets);

        // Act
        var result = await _sut.GetByStatusAsync("Open");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Open 1", result[0].Title);
        Assert.Equal("Open", result[0].Status);
    }

    [Fact]
    public async Task GetByStatusAsync_WhenStatusIsNullOrWhiteSpace_ReturnsEmptyList()
    {
        // Act
        var resultEmpty = await _sut.GetByStatusAsync("");
        var resultWhitespace = await _sut.GetByStatusAsync("   ");

        // Assert
        Assert.NotNull(resultEmpty);
        Assert.Empty(resultEmpty);
        Assert.NotNull(resultWhitespace);
        Assert.Empty(resultWhitespace);
        _ticketRepositoryMock.Verify(r => r.GetByStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenTicketExists_UpdatesStatusAndReturnsDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ticket = new SupportTicket
        {
            Id = id,
            Title = "Old",
            Description = "Desc",
            Status = "Open",
            Priority = "Medium",
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        _ticketRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<SupportTicket>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var request = new UpdateTicketRequest("Resolved", "High");

        // Act
        var result = await _sut.UpdateAsync(id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Resolved", result.Status);
        Assert.Equal("High", result.Priority);
        Assert.Equal("Resolved", ticket.Status);
        Assert.Equal("High", ticket.Priority);
        _ticketRepositoryMock.Verify(r => r.UpdateAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenTicketDoesNotExist_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((SupportTicket?)null);
        var request = new UpdateTicketRequest("Resolved", null);

        // Act
        var result = await _sut.UpdateAsync(id, request);

        // Assert
        Assert.Null(result);
        _ticketRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SupportTicket>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenTicketExists_ReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ticket = new SupportTicket { Id = id, Title = "To Delete", CustomerId = Guid.NewGuid(), Status = "Open", Priority = "Low" };
        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        _ticketRepositoryMock.Setup(r => r.DeleteAsync(It.IsAny<SupportTicket>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        Assert.True(result);
        _ticketRepositoryMock.Verify(r => r.DeleteAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenTicketDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _ticketRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((SupportTicket?)null);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        Assert.False(result);
        _ticketRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<SupportTicket>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
