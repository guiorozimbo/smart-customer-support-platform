using CustomerSupportPlatform.Application.DTOs;
using CustomerSupportPlatform.Application.Interfaces;
using CustomerSupportPlatform.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using CustomerService.Services;
using Xunit;

namespace CustomerService.Tests;

/// <summary>
/// Unit tests for <see cref="CustomerAppService"/> using mocked <see cref="ICustomerRepository"/>.
/// </summary>
public class CustomerAppServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<ILogger<CustomerAppService>> _loggerMock;
    private readonly CustomerAppService _sut;

    public CustomerAppServiceTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _loggerMock = new Mock<ILogger<CustomerAppService>>();
        _sut = new CustomerAppService(_customerRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerExists_ReturnsCustomerInformation()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customer = new Customer
        {
            Id = id,
            Name = "John Doe",
            Email = "john@example.com",
            Phone = "+5511999999999",
            DocumentNumber = "12345678900",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };
        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal("john@example.com", result.Email);
        Assert.Equal("+5511999999999", result.Phone);
        Assert.Equal("12345678900", result.DocumentNumber);
        _customerRepositoryMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerDoesNotExist_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _customerRepositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        Assert.Null(result);
        _customerRepositoryMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenCustomerExists_ReturnsCustomerInformation()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customer = new Customer
        {
            Id = id,
            Name = "Jane Smith",
            Email = "jane@company.com",
            Phone = null,
            DocumentNumber = null,
            CreatedAt = DateTime.UtcNow
        };
        _customerRepositoryMock
            .Setup(r => r.GetByEmailAsync("jane@company.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        // Act
        var result = await _sut.GetByEmailAsync("jane@company.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Jane Smith", result.Name);
        Assert.Equal("jane@company.com", result.Email);
        Assert.Null(result.Phone);
        Assert.Null(result.DocumentNumber);
        _customerRepositoryMock.Verify(r => r.GetByEmailAsync("jane@company.com", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenEmailIsNullOrWhiteSpace_ReturnsNull()
    {
        // Act
        var resultEmpty = await _sut.GetByEmailAsync("");
        var resultWhitespace = await _sut.GetByEmailAsync("   ");

        // Assert
        Assert.Null(resultEmpty);
        Assert.Null(resultWhitespace);
        _customerRepositoryMock.Verify(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsCreatedCustomerDto()
    {
        // Arrange
        var request = new CreateCustomerRequest("New User", "new@test.com", "+5511888888888", "98765432100");
        Customer? capturedCustomer = null;
        _customerRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .Callback<Customer, CancellationToken>((c, _) => capturedCustomer = c)
            .ReturnsAsync((Customer c, CancellationToken _) => c);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New User", result.Name);
        Assert.Equal("new@test.com", result.Email);
        Assert.Equal("+5511888888888", result.Phone);
        Assert.Equal("98765432100", result.DocumentNumber);
        Assert.NotNull(capturedCustomer);
        Assert.Equal("New User", capturedCustomer.Name);
        _customerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateAsync(null!));
        _customerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenCustomerExists_UpdatesAndReturnsDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customer = new Customer
        {
            Id = id,
            Name = "Old Name",
            Email = "old@test.com",
            CreatedAt = DateTime.UtcNow
        };
        _customerRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _customerRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var request = new UpdateCustomerRequest("New Name", "new@test.com", null, null);

        // Act
        var result = await _sut.UpdateAsync(id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        Assert.Equal("new@test.com", result.Email);
        Assert.Equal("New Name", customer.Name);
        _customerRepositoryMock.Verify(r => r.UpdateAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCustomerExists_ReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var customer = new Customer { Id = id, Name = "To Delete", Email = "del@test.com" };
        _customerRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        _customerRepositoryMock.Setup(r => r.DeleteAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        Assert.True(result);
        _customerRepositoryMock.Verify(r => r.DeleteAsync(customer, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenCustomerDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _customerRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Customer?)null);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        Assert.False(result);
        _customerRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
