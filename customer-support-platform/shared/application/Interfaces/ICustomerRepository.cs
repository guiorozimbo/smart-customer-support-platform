using CustomerSupportPlatform.Domain.Entities;

namespace CustomerSupportPlatform.Application.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Customer?> GetByDocumentAsync(string documentNumber, CancellationToken cancellationToken = default);
}
