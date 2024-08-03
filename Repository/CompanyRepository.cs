using Contracts.RepositoryContracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class CompanyRepository(RepositoryContext repositoryContext) : RepositoryBase<Company>(repositoryContext), ICompanyRepository
{
    public async Task<IEnumerable<Company>> GetAllCompaniesAsync(bool trackChanges)
    => await FindAll(trackChanges).OrderBy(c => c.Name).ToListAsync();

    public async Task<Company> GetCompanyAsync(Guid companyId, bool trackChanges)
    => await FindByCondition(c => c.Id.Equals(companyId), trackChanges).SingleOrDefaultAsync();

    public async Task<IEnumerable<Company>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges)
    => await FindByCondition(x => ids.Contains(x.Id), trackChanges).ToListAsync();

    public void CreateCompany(Company company) => Create(company);

    public void DeleteCompany(Company company) => Delete(company);
}