using CustomerSupportPlatform.Application.DTOs;
using CustomerSupportPlatform.Application.Interfaces;
using CustomerSupportPlatform.Domain.Entities;

namespace CustomerService.Services;

/// <summary>
/// Application service for customer operations. Delegates persistence to <see cref="ICustomerRepository"/>.
/// </summary>
public class CustomerAppService : ICustomerAppService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CustomerAppService> _logger;

    public CustomerAppService(ICustomerRepository customerRepository, ILogger<CustomerAppService> logger)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, ct);
        return customer is null ? null : MapToDto(customer);
    }

    public async Task<CustomerDto?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var customer = await _customerRepository.GetByEmailAsync(email.Trim(), ct);
        return customer is null ? null : MapToDto(customer);
    }

    public async Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken ct = default)
    {
        var customers = await _customerRepository.GetAllAsync(ct);
        return customers.Select(MapToDto).ToList();
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var customer = new Customer
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            DocumentNumber = request.DocumentNumber
        };
        var created = await _customerRepository.AddAsync(customer, ct);
        _logger.LogInformation("Customer created: {Email}, Id: {Id}", created.Email, created.Id);
        return MapToDto(created);
    }

    public async Task<CustomerDto?> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var customer = await _customerRepository.GetByIdAsync(id, ct);
        if (customer is null)
            return null;

        if (request.Name is { } name)
            customer.Name = name;
        if (request.Email is { } email)
            customer.Email = email;
        if (request.Phone is { } phone)
            customer.Phone = phone;
        if (request.DocumentNumber is { } doc)
            customer.DocumentNumber = doc;

        await _customerRepository.UpdateAsync(customer, ct);
        _logger.LogInformation("Customer updated: {Id}", id);
        return MapToDto(customer);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, ct);
        if (customer is null)
            return false;

        await _customerRepository.DeleteAsync(customer, ct);
        _logger.LogInformation("Customer deleted: {Id}", id);
        return true;
    }

    private static CustomerDto MapToDto(Customer c) => new(
        c.Id, c.Name, c.Email, c.Phone, c.DocumentNumber, c.CreatedAt);
}
