using CustomerSupportPlatform.Application.DTOs;

namespace CustomerSupportPlatform.Application.Interfaces;

/// <summary>
/// Application service for customer operations.
/// </summary>
public interface ICustomerAppService
{
    Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<CustomerDto?> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
