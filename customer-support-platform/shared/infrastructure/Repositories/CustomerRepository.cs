using CustomerSupportPlatform.Domain.Entities;
using CustomerSupportPlatform.Application.Interfaces;
using Database;
using Microsoft.EntityFrameworkCore;

namespace CustomerSupportPlatform.Infrastructure.Repositories;

public class CustomerRepository : RepositoryBase<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context) { }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await Context.Customers.FirstOrDefaultAsync(c => c.Email == email, cancellationToken);

    public async Task<Customer?> GetByDocumentAsync(string documentNumber, CancellationToken cancellationToken = default) =>
        await Context.Customers.FirstOrDefaultAsync(c => c.DocumentNumber == documentNumber, cancellationToken);
}
